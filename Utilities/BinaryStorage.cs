using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utilities
{
    /// <summary>
    /// Интерфейс работника с элементами в бинарном хранилище (BinaryStorageBase)
    /// </summary>
    /// <typeparam name="TBar"></typeparam>
    public interface IBarAdaptor<TBar>
    {
        // методы по сериализации
        int GetBarSize();   // Количество байт, занимаемое каждым объектом в файле (должно быть фиксированным!)
        //TBar[] Restore(BinaryReader br, int Len);
        TBar Restore(BinaryReader br);
        void Save(BinaryWriter bw, TBar bar);
        // временные характиеристики бара
        DateTime GetCloseTime(TBar bar);
        DateTime GetOpenTime(TBar bar);
        //DateTime GetLastTickTime(TBar bar);
    }

    public class Correction<TBar>
    {
        // при включенном флаге RemoveInterval - удалить данные за период IntervalBegin..IntervalEnd
        // при наличии BarsSequenceToInsert:
        //      удалить старые данные за период BarsSequenceToInsert:
        //      и вставить новые данные
        public bool RemoveInterval;
        public DateTime IntervalBegin;
        public DateTime IntervalEnd;
        public TBar[] BarsSequenceToInsert;
    }

    public interface IBinaryStorageReader<TBar> : IDisposable
    {
        bool IsEmpty { get; }
        DateTime EarliestBarOpenTime { get; }
        DateTime LatestBarCloseTime { get; }
        TBar GetBarStartedBeforeTime(DateTime time);

        IEnumerable<TBar> EnumerateBars(DateTime from, DateTime to);

        int Count { get; }
        TBar GetValue(int index);

        bool CheckRefresh();
        void Refresh();
    }


    public class BinaryStorage<TBar, TBarAdaptor> : IDisposable
        where TBar : class
        where TBarAdaptor : IBarAdaptor<TBar>
    {
        private const int DefContentsLen = 4096;
        private const int MinNumBarsInSegment = 256;
        private const int BestSegmentSize = 64*1024;

        /*
        Структура хранилища:
        Оглавление
          - размер оглавления=(4К по-умолчанию)
          - размер блоков данных=32К (по-умолчанию)
          - размер одной записи=зависит от данных
          - текстовое описание: (добавим позже)
             - длина описания
             - байт-поток описания
        Блоки данных по сегментам:
          - время (локальное), когда блок был добавлен (long)
          - время (локальное), кодга блок был удален   (long, 0=актуален)
          - количество записей в блоке (если 0-все данные до конца сегмента/файла; если >0 - конкретное количество (при коррекциях))
          - данные
         */
        public class Segment
        {
            public DateTime CreationTime;
            public DateTime InvalidationTime;

            public DateTime StartTime;
            public DateTime EndTime;
            public long FilePosition;
            public int NumRecords;
            public Segment() { }
            public Segment(Segment from)
            {
                StartTime = from.StartTime;
                EndTime = from.EndTime;
                FilePosition = from.FilePosition;
                NumRecords = from.NumRecords;
            }
            public TBar[] LoadSegment(FileStream fs, BinaryReader br, TBarAdaptor adaptor)
            {
                fs.Seek(FilePosition + SegmentTitleLen, SeekOrigin.Begin);
                var res = new TBar[NumRecords];
                for (int i = 0; i < NumRecords; ++i)
                    res[i] = adaptor.Restore(br);
                return res;
            }
            internal static int DefComparison(Segment x, Segment y)
            {
                return x.StartTime.CompareTo(y.StartTime);
            }
        }

        readonly SafeList<UpdateFlag> ReaderRefreshFlags = new();
        public class Reader : IBinaryStorageReader<TBar>
        {
            private readonly BinaryStorage<TBar, TBarAdaptor> _owner;
            private readonly TBarAdaptor Adaptor;
            private TBar[] AccumulatorCopy;

            private List<Segment> Segments;
            private FileStream fs;
            private BinaryReader br;
            private List<int> SegmIndicies;
            private int _countWithoutAccumulator;
            public int Count { get; private set; }

            private readonly UpdateFlag UpdateFlag;

            private readonly Cash<Segment,TBar[]> _cash=new();
            public Reader(BinaryStorage<TBar, TBarAdaptor> owner)
            {
                _owner = owner;
                UpdateFlag = new UpdateFlag();
                _owner.ReaderRefreshFlags.Add(UpdateFlag);

                fs = File.Open(_owner.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                br = new BinaryReader(fs);
                Adaptor = _owner.BarAdaptor;
                // копируем сегменты
                InitSegments();
                
            }
            public bool CheckRefresh()
            {
                if (!UpdateFlag.GetReset()) return false;
                Refresh();
                return true;
            }
            public void Refresh()
            {
                InitSegments();
            }
            private void InitSegments()
            {
                List<Segment> segments = _owner.mSegments.Select(segment => new Segment(segment)).ToList();
                AccumulatorCopy = _owner.Accumulator.ToArray();
                segments.Sort(Segment.DefComparison);

                int count = 0;
                var segmIndicies = new List<int>();
                foreach (Segment segment in segments)
                {
                    segmIndicies.Add(count);
                    count += segment.NumRecords;
                }
                
                // порядок присвоения расчитан на то, чтобы иметь потокобезопасное чтение данных при параллельном вызове Refresh из другого потока
                Segments = segments;
                SegmIndicies = segmIndicies;
                _countWithoutAccumulator = count;
                Count = count + AccumulatorCopy.Length;
                UpdateDataRange();
            }

            public void Close()
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                    br = null;

                    _owner.ReaderRefreshFlags.Remove(UpdateFlag);
                }
            }
            public void Dispose()
            {
                Close();
            }
            private TBar[] LoadSegmentData(Segment s)
            {
                return s.LoadSegment(fs, br, Adaptor);
            }
            public bool IsEmpty
            {
                get { return Count == 0; }
            }
            public DateTime EarliestBarOpenTime { get; private set; }
            private DateTime _getEarliestBarOpenTime()
            {
                if (Count == 0) return DateTime.MinValue;
                if (_countWithoutAccumulator > 0)
                    return Segments[0].StartTime;
                return OpenTime(AccumulatorCopy[0]);
            }
            public DateTime LatestBarCloseTime { get; private set; }
            private DateTime _getLatestBarCloseTime()
            {
                if (Count == 0) return DateTime.MinValue;
                if (AccumulatorCopy.Length > 0)
                    return CloseTime(AccumulatorCopy[AccumulatorCopy.Length - 1]);
                return Segments[Segments.Count - 1].EndTime;
            }
            private void UpdateDataRange()
            {
                EarliestBarOpenTime = _getEarliestBarOpenTime();
                LatestBarCloseTime = _getLatestBarCloseTime();
            }


            public TBar GetBarStartedBeforeTime(DateTime time)
            {
                if (AccumulatorCopy.Length > 0 && OpenTime(AccumulatorCopy[0])<=time)
                    return AccumulatorCopy.Last(b => Adaptor.GetOpenTime(b) <= time);
                    
                if (Segments.Count == 0) return null;
                Segment segment=Segments.LastOrDefault(s => s.StartTime <= time) ?? Segments[0];

                TBar[] segmentBars = _cash.Get(segment);
                if (segmentBars == null)
                    _cash.Set(segment, segmentBars = LoadSegmentData(segment));

                return segmentBars.Last(b => Adaptor.GetOpenTime(b) <= time) ?? segmentBars.First();
            }
            /// <summary>
            /// Перечислить бары за указанный интервал 
            /// (порядок возрастающий; бары, попавшие в интервал частично также передаются)
            /// </summary>
            public IEnumerable<TBar> EnumerateBars(DateTime from, DateTime to)
            {
                // определяем сегменты к передаче:
                //    первый сегмент: CloseTime пред.сегмента < from
                //    последний сегмент: OpenTime след.сегмента> to
                int iFirstSegment = 1 + Segments.FindNearestItem(BinarySearchTarget.Less, from, segment => segment.EndTime);
                int iLastSegment = Segments.FindNearestItem(BinarySearchTarget.More, to, segment => segment.StartTime);
                if (iLastSegment < 0)
                    iLastSegment = Segments.Count; //до концаплюс аккумулятор
                else
                    --iLastSegment;

                for (int iSegment = iFirstSegment; iSegment <= iLastSegment; ++iSegment)
                {
                    TBar[] segmentBars;
                    if (iSegment < Segments.Count)
                        segmentBars = LoadSegmentData(Segments[iSegment]);
                    else if (AccumulatorCopy.Length == 0) 
                        break;
                    else
                        segmentBars = AccumulatorCopy;
                    
                    // Определяем бары сегмента к передаче
                    int iFirstBar;
                    if (iSegment > iFirstSegment)
                        iFirstBar = 0;
                    else
                    {
                        // первый бар, для которого выполнено условие bar.OpenTime>=from
                        iFirstBar = BinarySearch.FindNearestItem(segmentBars, BinarySearchTarget.MoreEq, from, segmentBars.Length,
                                                                (time, bars, index) =>
                                                                time.CompareTo(OpenTime(bars[index])));
                        // добавление частино-вошедшего пред.бара
                        if (iFirstBar > 0 && CloseTime(segmentBars[iFirstBar - 1]) > from)
                            --iFirstBar;
                    }
                    int iLastBar;
                    if (iSegment < iLastSegment)
                        iLastBar = segmentBars.Length - 1;
                    else
                    {
                        // последний бар, для которого выполнено условие bar.CloseTime<=to
                        iLastBar = BinarySearch.FindNearestItem(segmentBars,BinarySearchTarget.LessEq, to, segmentBars.Length,
                                                                (time, bars, index) =>
                                                                time.CompareTo(CloseTime(bars[index])));
                        // добавление частино-вошедшего след.бара
                        if (iLastBar + 1 < segmentBars.Length && OpenTime(segmentBars[iLastBar + 1]) < to)
                            ++iLastBar;
                    }
                    if (iFirstBar >= 0 && iLastBar>= iFirstBar)
                        for (int i = iFirstBar; i <= iLastBar; ++i)
                        {
                            yield return segmentBars[i];
                        }
                }
            }

            public TBar GetValue(int index)
            {
                if (index<0 ||index>=Count) return default(TBar);
                if (index >= _countWithoutAccumulator)
                    return AccumulatorCopy[index - _countWithoutAccumulator];

                int ixSegm = SegmIndicies.FindNearestItem(BinarySearchTarget.LessEq, index, v => v);
                if (ixSegm < 0) return default(TBar);
                Segment segment = Segments[ixSegm];
                int ixBarInSegment = index - SegmIndicies[ixSegm];

                TBar[] segmentBars = _cash.Get(segment);
                if (segmentBars == null)
                {
                    _cash.Set(segment, segmentBars = LoadSegmentData(segment));
                }

                return segmentBars[ixBarInSegment];
            }
            private DateTime OpenTime(TBar bar)
            {
                return Adaptor.GetOpenTime(bar);
            }
            private DateTime CloseTime(TBar bar)
            {
                return Adaptor.GetCloseTime(bar);
            }
        }

        private readonly TBarAdaptor BarAdaptor;
        public readonly string FileName;
        //public readonly string Description;

        private FileStream mFileStream;
        private BinaryWriter bw;
        private BinaryReader br;
        public readonly bool IsReadOnly;
        // характеристики файла
        private readonly int mRecordLen;

        private int SegmentLength;
        private int ContentsLen;
        private int NumBarsInSegment;
        private const int CorrectionMarkerShift = sizeof(int) * 3;

        public int MaxAdditionsBeforeFlush { get; set; }

        // актуальный список сегментов
        private readonly List<Segment> mSegments = new();
        // все сегменты, включая удаленные при вводе корректировок
        private readonly List<Segment> mAllSegments = new();
        // буффер новых баров на запись
        private readonly List<TBar> Accumulator = new();
        // текущий недозаписанный сегмент
        private Segment CurrentSegment;


        #region Read/Write segment title
        private const int SegmentTitleLen = sizeof(long) * 2 + sizeof(int);
        private const int SegmentShift_Invalidation = sizeof(long);
        private const int SegmentShift_NumRecords = sizeof(long) * 2;
        private static void WriteSegmentTitle(BinaryWriter binw, long creationTime_ToBinary, DateTime invalidationTime, int numRecords)
        {
            binw.Write(creationTime_ToBinary);
            binw.Write(invalidationTime.ToBinary());
            binw.Write(numRecords);
        }
        private static void ReadSegmentTitle(BinaryReader br, out DateTime creationTime, out DateTime invalidationTime, out int numRecords)
        {
            creationTime = DateTime.FromBinary(br.ReadInt64());
            invalidationTime = DateTime.FromBinary(br.ReadInt64());
            numRecords = br.ReadInt32();
        }

        #endregion

        public BinaryStorage(TBarAdaptor barAdaptor, string fileName, bool bReadOnly)
        {
            BarAdaptor = barAdaptor;
            mRecordLen = BarAdaptor.GetBarSize();
            FileName = fileName;
            IsReadOnly = bReadOnly;
            MaxAdditionsBeforeFlush = 20;
            try
            {
                if (!File.Exists(FileName))
                {
                    if (bReadOnly) throw new Exception("File not found " + fileName);
                    CreateFile();
                }
                else
                    Open();

            }
            catch
            {
                Close();
                throw;
            }
        }
        private DateTime OpenTime(TBar bar)
        {
            return BarAdaptor.GetOpenTime(bar);
        }
        private DateTime CloseTime(TBar bar)
        {
            return BarAdaptor.GetCloseTime(bar);
        }
        private void CreateFile()
        {
            ContentsLen = DefContentsLen;

            NumBarsInSegment = BestSegmentSize/BarAdaptor.GetBarSize();
            if (NumBarsInSegment < MinNumBarsInSegment)
                NumBarsInSegment = MinNumBarsInSegment;

            mFileStream = File.Open(FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            bw = new BinaryWriter(mFileStream);
            br = new BinaryReader(mFileStream);
            // размер оглавления
            bw.Write(ContentsLen);
            // размер сегмента
            SegmentLength = SegmentTitleLen + NumBarsInSegment * mRecordLen;
            bw.Write(SegmentLength);
            // размер записи
            bw.Write(mRecordLen);
            // ReSharper disable ConvertToConstant.Local
            long correctionMarker = 0;
            bw.Write(correctionMarker); //!!! Пишем long!!!
            //текстовое описание
            int descrLen = 0; // текстовое описание добавим позже
            // ReSharper restore ConvertToConstant.Local
            bw.Write(descrLen);
            // на конец оглавления
            mFileStream.SetLength(ContentsLen);
            mFileStream.Seek(ContentsLen, SeekOrigin.Begin);
            mFileStream.Flush();
        }
        private void Open()
        {
            if (IsReadOnly)
            {
                mFileStream = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                br = new BinaryReader(mFileStream);
            }
            else
            {
                mFileStream = File.Open(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                bw = new BinaryWriter(mFileStream);
                br = new BinaryReader(mFileStream);
            }
            long fileLength = mFileStream.Length;

            #region Читаем оглавление
            // размер оглавления
            if (fileLength < sizeof(int)) throw new Exception("File is corrupted");
            ContentsLen = br.ReadInt32();
            if (mFileStream.Length < ContentsLen) throw new Exception("File is corrupted");
            // размер сегмента
            SegmentLength = br.ReadInt32();
            // размер записи
            int recordLen = br.ReadInt32();
            // маркер незавершенной транзакции по коррекции
            long correctionMarker = br.ReadInt64();

            // проверки:
            // минимальные допустимые размеры
            if (recordLen <= 0 || SegmentLength <= SegmentTitleLen) throw new Exception("File is corrupted");
            NumBarsInSegment = (SegmentLength - SegmentTitleLen) / recordLen;
            // размер сегмента кратен количеству записей + 2 DateTime
            if (NumBarsInSegment <= 0 || NumBarsInSegment * recordLen + SegmentTitleLen != SegmentLength)
                throw new Exception("File is corrupted");
            // размер записи должен соответствовать адаптору
            if (recordLen != mRecordLen)
                throw new Exception("BarAdaptor isinvalid");
            #endregion
            #region Считываем сегменты

            for (long position = ContentsLen; position < fileLength; position += SegmentLength)
            {
                mFileStream.Seek(position, SeekOrigin.Begin);

                var segmSize = (int)(fileLength - position);
                if (segmSize > SegmentLength) segmSize = SegmentLength;
                int barsSize = segmSize - SegmentTitleLen;
                // если запись сегмента была оборвана                
                if (barsSize < recordLen) // хотя бы одна запись должна была быть сохранена
                {
                    if (!IsReadOnly)
                        mFileStream.SetLength(position);
                    break;
                }
                // определяем, сколько баров в сегменте
                int numBars = barsSize / recordLen; // !округление вниз

                // считываем шапку
                ReadSegmentTitle(br, out var creationTime, out var invalidationTime, out var fixedNumBars);

                if (fixedNumBars > numBars) 
                {
                    // fixedNumBars может быть ненулевым только когда мы закрываем недописанный сегмент
                    // (см.метод CloseNotFinishedSegment)
                    // данные сегмента перед этим флэшатся
                    // здесь мы имеем инфу, что к-во баров, объявленное в шапке больше, чем фактическое
                    throw new Exception("File is corrupted");
                    // вариант решения проблемы:  fixedNumBars:=numBars
                    // (с долей вероятности мы теряем данные)
                    // главное - с файлом что-то не так!
                }
                if (fixedNumBars != 0)
                    numBars = fixedNumBars;

                if (fixedNumBars != 0 && fileLength < position + SegmentLength)
                {
                    // если приложение упало в момент закрытия сегмента
                    // выровнять файл по границе сегмента
                    if (!IsReadOnly)
                        mFileStream.SetLength(position + SegmentLength);
                }
                // считываем первый и последний бары сегмента
                TBar firstBar = BarAdaptor.Restore(br);
                TBar lastBar;
                if (numBars == 1)
                    lastBar = firstBar;
                else
                {
                    mFileStream.Seek(position + SegmentTitleLen + recordLen * (numBars - 1), SeekOrigin.Begin);
                    lastBar = BarAdaptor.Restore(br);
                }
                var s = new Segment
                {
                    FilePosition = position,
                    StartTime = OpenTime(firstBar),
                    EndTime = CloseTime(lastBar),
                    CreationTime = creationTime,
                    InvalidationTime = invalidationTime,
                    NumRecords = numBars
                };
                mAllSegments.Add(s);
                if (invalidationTime == DateTime.MaxValue)
                    mSegments.Add(s);

                if (numBars < NumBarsInSegment && fixedNumBars == 0) // последний недописанный сегмент
                {
                    if (invalidationTime != DateTime.MaxValue)
                    {
                        // недописанный сегмент не может быть отмененным
                        throw new Exception("File is corrupted");
                        // как вариант решения - обрезать файл по началу сегмента,
                        // полностью его проигнорировав: 
                        //    1) fsWrite.SetLength(position + SegmentLength);
                        //    2) mAllSegments.Remove(s); mSegments.Remove(s);
                        // (теряем информацию о том, что было до коррекции)
                        // главное - с файлом что-то не так!
                    }

                    if (!IsReadOnly)
                    {
                        long actualFileLen = position + SegmentTitleLen + numBars * recordLen;
                        if (actualFileLen < fileLength) // последний сохраняемый бар был обрезан
                        {
                            // укорачиваем файл
                            mFileStream.SetLength(actualFileLen);
                        }
                        CurrentSegment = s;
                    }
                    break;
                }
            }
            #endregion
            if (correctionMarker == 0)
            {
                // позиционируемся в конец
                mSegments.Sort(Segment.DefComparison);
                if (mSegments.Count > 0)
                {
                    LatestTime = mSegments[mSegments.Count - 1].EndTime;
                    EarliestTime = mSegments[0].StartTime;
                }
                if (!IsReadOnly)
                    mFileStream.Seek(mFileStream.Length, SeekOrigin.Begin);
            }
            else // Обнаружена незавершенная транзакция по коррекции!!!
                RollbackBrokenCorrection(correctionMarker);
        }
        private void CorrectionMarker(long marker)
        {
            mFileStream.Seek(CorrectionMarkerShift, SeekOrigin.Begin);
            bw.Write(marker);
        }

        public void Close()
        {
            if (mFileStream != null)
            {
                FlushImpl();
                mFileStream.Close();
                mFileStream = null;
                bw = null;
            }
        }
        public void Dispose()
        {
            Close();
        }

        public void AddBar(TBar bar)
        {
            if (IsReadOnly) throw new Exception("File is opened with ReadOnly mode");

            DateTime closeTime = CloseTime(bar);

            if (LatestTime >= closeTime) throw new Exception("Bar times must be monotonically increasing");
            DateTime openTime = OpenTime(bar);
            if (LatestTime > openTime || openTime > closeTime) throw new Exception("Bar times must be monotonically increasing or Invalid bar open time");
            Accumulator.Add(bar);
            LatestTime = closeTime;
            if (EarliestTime == DateTime.MinValue)
                EarliestTime = openTime;

            if (Accumulator.Count >= MaxAdditionsBeforeFlush)
                FlushImpl();

            NotifyReaders();
        }

        private void NotifyReaders()
        {
            foreach (var item in ReaderRefreshFlags.GetItems())
                item.Set();
        }
        public DateTime LatestTime { get; private set; }
        public DateTime EarliestTime { get; private set; } // openTime of the earliest bar

        public void Flush()
        {
            FlushImpl();
            NotifyReaders();
        }
        private void FlushImpl()
        {
            if (IsReadOnly) return;
            if (Accumulator.Count == 0) return;
            WriteSequence(Accumulator, 0);
            mFileStream.Flush();
        }
        private void WriteSequence(List<TBar> sequence, long corrMarker)
        {
            while (sequence.Count > 0)
            {
                if (CurrentSegment == null)
                {
                    CurrentSegment = CreateNewSegment(corrMarker);
                    CurrentSegment.StartTime = OpenTime(sequence[0]);
                    mSegments.Add(CurrentSegment);
                    mAllSegments.Add(CurrentSegment);
                }
                int itemsToSave = Math.Min(NumBarsInSegment - CurrentSegment.NumRecords, sequence.Count);
                // пишем данные
                for (int i = 0; i < itemsToSave; ++i)
                    BarAdaptor.Save(bw, sequence[i]);
                CurrentSegment.NumRecords += itemsToSave;
                CurrentSegment.EndTime = CloseTime(sequence[itemsToSave - 1]);
                if (CurrentSegment.NumRecords == NumBarsInSegment)
                {
                    CurrentSegment = null;
                }
                sequence.RemoveRange(0, itemsToSave);
            }
        }
        private Segment CreateNewSegment(long corrMarker)
        {
            DateTime creationTime;
            if (corrMarker == 0)
            {
                creationTime = DateTime.UtcNow;
                corrMarker = creationTime.ToBinary();
            }
            else
                creationTime = DateTime.FromBinary(corrMarker);

            var res = new Segment
            {
                FilePosition = mFileStream.Position,
                CreationTime = creationTime,
                InvalidationTime = DateTime.MaxValue,
                NumRecords = 0,
            };
            WriteSegmentTitle(bw, corrMarker, DateTime.MaxValue, 0);
            return res;
        }
        private void CloseNotFinishedSegment()
        {
            if (CurrentSegment == null) return;
            // фиксируем количество записей в сегменте
            mFileStream.Flush();
            mFileStream.Seek(CurrentSegment.FilePosition + SegmentShift_NumRecords, SeekOrigin.Begin);
            bw.Write(CurrentSegment.NumRecords);
            // выравниваем файл по границе сегмента
            long newFileLen = CurrentSegment.FilePosition + SegmentLength;
            mFileStream.SetLength(newFileLen);
            mFileStream.Seek(newFileLen, SeekOrigin.Begin);
            CurrentSegment = null;
        }


        /// <summary>
        ///  Получать в потоке писателя, пользоваться - где угодно
        /// </summary>
        public IBinaryStorageReader<TBar> CreateReader()
        {
            if (mFileStream == null) return null;
            return new Reader(this);
        }

        public void ApplyCorrections(IEnumerable<Correction<TBar>> corrections)
        {
            if (IsReadOnly) throw new Exception("File is opened with ReadOnly mode");
            // сбрасываем аккумулятор на диск
            FlushImpl();
            // закрываем последний сегмент
            CloseNotFinishedSegment();

            // формируем предметную часть коррекции 
            // какие сегменты убираем, какие данные вставляем
            var dataBlocks = new List<DataBlock>();
            var segmentsToInvalidate = new List<Segment>();
            PrepareCorrections(corrections, dataBlocks, segmentsToInvalidate);

            // если подготовка прошла успешно (Exception не произошел)
            // фиксируем время корректировки
            DateTime correctionTime = DateTime.UtcNow;
            long corrMarker = correctionTime.ToBinary();
            // выставляем в файле флаг начатой транзакции
            CorrectionMarker(corrMarker);
            // объявляем сегменты к удалению невалидными
            foreach (Segment segment in segmentsToInvalidate)
                InvalidateSegment(corrMarker, segment);
            // пишем данные к вставке, перестраиваем mSegments
            CreateCorrectionsSegments(corrMarker, dataBlocks);
            // сбрасываем флаг транзакции
            CorrectionMarker(0);

            int NumSegments = mSegments.Count;
            if (NumSegments == 0)
                LatestTime = EarliestTime = DateTime.MinValue;
            else
            {
                EarliestTime = mSegments[0].StartTime;
                LatestTime = mSegments[NumSegments - 1].EndTime;
            }

            mFileStream.Flush();
            // позиционируемся в конец файла
            mFileStream.Seek(mFileStream.Length, SeekOrigin.Begin);
            NotifyReaders();
        }
        #region Corrections implementation
        class DataBlock
        {
            public DateTime Begin;
            public DateTime End;
            public Segment Segm;
            public List<TBar> Data;
        }
        private void PrepareCorrections(IEnumerable<Correction<TBar>> corrections, List<DataBlock> dataBlocks, List<Segment> segmentsToInvalidate)
        {
            // создаем список блоков
            dataBlocks.AddRange(
                mSegments.Select(s => new DataBlock
                {
                    Segm = s,
                    Begin = s.StartTime,
                    End = s.EndTime
                }));
            // последовательно по списку коррекций:
            foreach (Correction<TBar> corr in corrections)
            {
                // если задан интервал к вырезанию
                if (corr.RemoveInterval && corr.IntervalBegin < corr.IntervalEnd)
                    PrepareCorrectionsItem(
                        dataBlocks, segmentsToInvalidate,
                        corr.IntervalBegin, corr.IntervalEnd, null);
                // если задан интервал к замещению
                TBar[] toInsert = corr.BarsSequenceToInsert;
                if (toInsert != null && toInsert.Length > 0)
                {
                    CheckValidTimes(toInsert);
                    PrepareCorrectionsItem(
                        dataBlocks, segmentsToInvalidate,
                        OpenTime(toInsert[0]),
                        CloseTime(toInsert[toInsert.Length - 1]),
                        toInsert);
                }
            }
        }

        private void CheckValidTimes(IEnumerable<TBar> bars)
        {
            DateTime tm = DateTime.MinValue;
            foreach (TBar bar in bars)
            {
                DateTime openTime = OpenTime(bar);
                DateTime closeTime = CloseTime(bar);
                if (tm >= closeTime ||
                    tm > openTime ||
                    openTime > closeTime)
                    throw new Exception();
            }
        }
        private void PrepareCorrectionsItem(List<DataBlock> dataBlocks, List<Segment> segmentsToInvalidate,
            DateTime intervalBegin, DateTime intervalEnd, TBar[] dataToInsert)
        {
            int L = dataBlocks.Count;

            #region добавление новых данных в конец
            if (L == 0 || intervalBegin >= dataBlocks[L - 1].End)
            {
                if (dataToInsert != null)
                    dataBlocks.Add(new DataBlock
                    {
                        Begin = OpenTime(dataToInsert[0]),
                        End = CloseTime(dataToInsert[dataToInsert.Length - 1]),
                        Data = new List<TBar>(dataToInsert)
                    });
                return;
            }
            #endregion
            #region  вставка новых данных в начало
            if (intervalEnd <= dataBlocks[0].Begin)
            {
                if (dataToInsert != null)
                    dataBlocks.Insert(0, new DataBlock
                    {
                        Begin = OpenTime(dataToInsert[0]),
                        End = CloseTime(dataToInsert[dataToInsert.Length - 1]),
                        Data = new List<TBar>(dataToInsert)
                    });
                return;
            }
            #endregion
            #region  замещение посередине файла
            // находим блоки,относящиеся к интервалу замены
            int iFirstBlock = FindFirstBlock(dataBlocks, intervalBegin);
            int iLastBlock = FindLastBlock(dataBlocks, intervalEnd);
            if (iLastBlock < iFirstBlock) // вставляемые данные находятся между блоками
            {
                if (dataToInsert != null)
                    dataBlocks.Insert(iFirstBlock, new DataBlock
                    {
                        Begin = OpenTime(dataToInsert[0]),
                        End = CloseTime(dataToInsert[dataToInsert.Length - 1]),
                        Data = new List<TBar>(dataToInsert)
                    });
                return;

            }

            if (iFirstBlock == iLastBlock) // все изменения касаются одного сегмента
            {
                // если сегмент, то загрузить данные, сегмент - в список к удалению
                DataBlock block = dataBlocks[iFirstBlock];
                if (block.Segm != null)
                {
                    block.Data = LoadSegmentData(block.Segm);
                    segmentsToInvalidate.Add(block.Segm);
                    block.Segm = null;
                }
                // выясняем интервал вырезания
                int iReplaceBegin = 1 + GetIndexBeforeReplace(block, intervalBegin);
                int iFirstAfterReplace = GetIndexAfterReplace(block, intervalEnd);
                if (iReplaceBegin < iFirstAfterReplace)
                {
                    // если есть чего вырезать
                    block.Data.RemoveRange(iReplaceBegin, iFirstAfterReplace - iReplaceBegin);
                }
                // если есть чего вставлять
                if (dataToInsert != null)
                    block.Data.InsertRange(iReplaceBegin, dataToInsert);

                // блок опустел- исключить
                if (block.Data.Count == 0)
                    dataBlocks.RemoveAt(iFirstBlock);

                return;
            }

            // ОБЩИЙ СЛУЧАЙ
            // загрузить краевые сегменты, исключить промежуточные блоки
            for (int i = iFirstBlock; i <= iLastBlock; ++i)
            {
                DataBlock block = dataBlocks[iFirstBlock];
                if (i == iFirstBlock || i == iLastBlock)
                {
                    if (block.Segm != null)
                    {
                        block.Data = LoadSegmentData(block.Segm);
                        segmentsToInvalidate.Add(block.Segm);
                        block.Segm = null;
                    }
                }
                else
                {
                    if (block.Segm != null)
                        segmentsToInvalidate.Add(block.Segm);

                    dataBlocks.RemoveAt(i--);
                    --iLastBlock;
                }
            }

            // вырезаем края блоков
            DataBlock firstBlock = dataBlocks[iFirstBlock];
            int _iReplaceBegin = 1 + GetIndexBeforeReplace(firstBlock, intervalBegin);
            if (_iReplaceBegin < firstBlock.Data.Count)
                firstBlock.Data.RemoveRange(_iReplaceBegin, firstBlock.Data.Count - _iReplaceBegin);

            DataBlock lastBlock = dataBlocks[iLastBlock];
            int _iFirstAfterReplace = GetIndexAfterReplace(lastBlock, intervalEnd);
            if (_iFirstAfterReplace > 0)
                lastBlock.Data.RemoveRange(0, _iFirstAfterReplace);

            // добавляем к концу первого блока данные к вставке
            if (dataToInsert != null)
                firstBlock.Data.AddRange(dataToInsert);

            // исключаем опустевшие блоки
            if (lastBlock.Data.Count == 0)
                dataBlocks.RemoveAt(iLastBlock);
            if (firstBlock.Data.Count == 0)
                dataBlocks.RemoveAt(iFirstBlock);
            #endregion
        }
        /// <summary>
        /// Возвращаем индекс блока, такой чтобы для пред.блока было выполнено условие: Block.EndTime LessEq time
        /// </summary>
        private static int FindFirstBlock(List<DataBlock> dataBlocks, DateTime time)
        {
            int i = dataBlocks.FindNearestItem(BinarySearchTarget.LessEq, time, block => block.End);
            return i + 1;

        }
        /// <summary>
        /// Возвращаем индекс блока, такой чтобы для след.блока было выполнено условие: Block.BeginTime >= time
        /// </summary>
        private static int FindLastBlock(List<DataBlock> dataBlocks, DateTime time)
        {
            int i = dataBlocks.FindNearestItem(BinarySearchTarget.MoreEq, time, block => block.Begin);
            return (i < 0) ? (dataBlocks.Count - 1) : (i - 1);
        }
        /// <summary>
        /// Возвращает индекс элемента в блоке, последнего перед интервалом коррекции (-1 если с самого начала)
        /// </summary>
        private int GetIndexBeforeReplace(DataBlock block, DateTime time)
        {
            return block.Data.FindNearestItem(BinarySearchTarget.LessEq, time, CloseTime);
        }
        /// <summary>
        /// Возвращает индекс элемента в блоке, первого после интервала коррекции (block.Data.Count если до самого конца)
        /// </summary>
        private int GetIndexAfterReplace(DataBlock block, DateTime time)
        {
            int res = block.Data.FindNearestItem(BinarySearchTarget.MoreEq, time, OpenTime);
            return (res < 0) ? block.Data.Count : res;
        }
        private List<TBar> LoadSegmentData(Segment s)
        {
            return new(s.LoadSegment(mFileStream, br, BarAdaptor));
        }
        private void InvalidateSegment(long corrMarker, Segment segment)
        {
            mFileStream.Seek(segment.FilePosition + SegmentShift_Invalidation, SeekOrigin.Begin);
            bw.Write(corrMarker);
        }
        private void CreateCorrectionsSegments(long corrMarker, List<DataBlock> dataBlocks)
        {
            mSegments.Clear();
            mFileStream.Seek(mFileStream.Length, SeekOrigin.Begin);
            foreach (DataBlock block in dataBlocks)
            {
                // корректировка не касается этого сегмента
                if (block.Segm != null)
                {
                    if (CurrentSegment != null)
                        CloseNotFinishedSegment();

                    mSegments.Add(block.Segm);
                    continue;
                }
                WriteSequence(block.Data, corrMarker);
            }
            if (CurrentSegment != null)
                CloseNotFinishedSegment();

            //mSegments.Sort(Segment.DefComparison);
        }
        private void RollbackBrokenCorrection(long corrMarker)
        {
            // действуем в порядке,обратном проведению коррекции:
            // удаляем сегменты,добавленные коррекцией
            DateTime corrTime = DateTime.FromBinary(corrMarker);
            RemoveCorrectionsSegments(corrTime);
            // восстанавливаем сегменты,удаленные при корректировке
            foreach (Segment segment in mAllSegments)
                if (segment.InvalidationTime == corrTime)
                {
                    segment.InvalidationTime = DateTime.MaxValue;
                    if (!IsReadOnly)
                        InvalidateSegment(DateTime.MaxValue.ToBinary(), segment);
                }
            if (!IsReadOnly)
            {
                // сбрасываем флаг транзакции
                CorrectionMarker(0);
                // позиционируемся в конец файла
                mFileStream.Seek(mFileStream.Length, SeekOrigin.Begin);
                mFileStream.Flush();
            }
            // перестроить список активных сегментов
            mSegments.Clear();
            foreach (Segment segment in mAllSegments)
                if (segment.InvalidationTime == DateTime.MaxValue)
                    mSegments.Add(segment);
            mSegments.Sort(Segment.DefComparison);

            int NumSegments = mSegments.Count;
            if (NumSegments == 0)
                LatestTime = EarliestTime = DateTime.MinValue;
            else
            {
                EarliestTime = mSegments[0].StartTime;
                LatestTime = mSegments[NumSegments - 1].EndTime;
            }
        }
        private void RemoveCorrectionsSegments(DateTime corrTime)
        {
            // находим первый сегмент, вставленный при корректировке
            // убедиться, что все следующие за ним сегменты относятся к этой же корректировке
            int iFirstCorrectionSegment = -1;
            for (int i = 0; i < mAllSegments.Count; ++i)
            {
                Segment s = mAllSegments[i];
                if (iFirstCorrectionSegment < 0)
                {
                    if (s.CreationTime == corrTime)
                        iFirstCorrectionSegment = i;
                }
                else if (s.CreationTime != corrTime)
                {
                    // Недопустимая ситуация:
                    // Мы имеем незавершенную транзакцию по коррекции,
                    // при этом в конце файла находятся сегменты, не относящиеся к коррекции
                    throw new Exception("File is corrupted");
                }
            }
            if (iFirstCorrectionSegment < 0) return; // если таких сегментов нет, выход
            if (IsReadOnly)
            {
                // удаляем сегменты, добавленные при коррекции:
                // укорачиваем файл
                mFileStream.SetLength(mAllSegments[iFirstCorrectionSegment].FilePosition);
            }
            // убираем сегменты из оглавления
            mAllSegments.RemoveRange(iFirstCorrectionSegment, mAllSegments.Count - iFirstCorrectionSegment);
        }
        #endregion
    }
}
