using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Configurator
{
    /// <summary>
    /// Описание свойства
    /// </summary>
    public class PropertySpec
    {
        private Attribute[] attributes;
        private string category;
        private object defaultValue;
        private string description;
        private string editor;
        private object[] editorAttributes;
        private string name;
        private string type;
        private string typeConverter;
        private bool readOnly = false;


        #region Constructors
        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, string type) : this(name, type, null, null, null) { }

        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, Type type) :
            this(name, type.AssemblyQualifiedName, null, null, null)
        { }

        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, string type, string category) : this(name, type, category, null, null) { }

        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, Type type, string category) :
            this(name, type.AssemblyQualifiedName, category, null, null)
        { }

        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, string type, string category, string description) :
            this(name, type, category, description, null)
        { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, Type type, string category, string description) :
            this(name, type.AssemblyQualifiedName, category, description, null)
        { }

        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, string type, string category, string description, object defaultValue)
        {
            this.name = name;
            this.type = type;
            this.category = category;
            this.description = description;
            this.defaultValue = defaultValue;
            this.attributes = null;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, Type type, string category, string description, object defaultValue) :
            this(name, type.AssemblyQualifiedName, category, description, defaultValue)
        { }

        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, string type, string category, string description, object defaultValue,
            string editor, string typeConverter)
            : this(name, type, category, description, defaultValue)
        {
            this.editor = editor;
            this.typeConverter = typeConverter;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, Type type, string category, string description, object defaultValue,
            string editor, string typeConverter) :
            this(name, type.AssemblyQualifiedName, category, description, defaultValue, editor, typeConverter)
        { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, string type, string category, string description, object defaultValue,
            Type editor, string typeConverter) :
            this(name, type, category, description, defaultValue, editor.AssemblyQualifiedName,
            typeConverter)
        { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, Type type, string category, string description, object defaultValue,
            Type editor, string typeConverter) :
            this(name, type.AssemblyQualifiedName, category, description, defaultValue,
            editor.AssemblyQualifiedName, typeConverter)
        { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, Type type, string category, string description,
            Type editor, object[] EditorAttributes) :
            this(name, type, category, description)
        {
            this.editor = editor.AssemblyQualifiedName;
            this.editorAttributes = EditorAttributes;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, Type type, string category, string description,
            Type editor) :
            this(name, type, category, description)
        {
            this.editor = editor.AssemblyQualifiedName;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, string type, string category, string description, object defaultValue,
            string editor, Type typeConverter) :
            this(name, type, category, description, defaultValue, editor, typeConverter.AssemblyQualifiedName)
        { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, Type type, string category, string description, object defaultValue,
            string editor, Type typeConverter) :
            this(name, type.AssemblyQualifiedName, category, description, defaultValue, editor,
            typeConverter.AssemblyQualifiedName)
        { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, string type, string category, string description, object defaultValue,
            Type editor, Type typeConverter) :
            this(name, type, category, description, defaultValue, editor.AssemblyQualifiedName,
            typeConverter.AssemblyQualifiedName)
        { }
        /// <summary>
        /// Конструктор
        /// </summary>
        public PropertySpec(string name, Type type, string category, string description, object defaultValue,
            Type editor, Type typeConverter) :
            this(name, type.AssemblyQualifiedName, category, description, defaultValue,
            editor.AssemblyQualifiedName, typeConverter.AssemblyQualifiedName)
        { }
        #endregion

        /// <summary>
        /// Атрибуты
        /// </summary>
        public Attribute[] Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }
        /// <summary>
        /// Категория
        /// </summary>
        public string Category
        {
            get { return category; }
            set { category = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the propety can be edited.
        /// </summary>
        public bool ReadOnly
        {
            get { return readOnly; }
            set { readOnly = value; }
        }

        /// <summary>
        /// Gets or sets converter type name.
        /// </summary>
        public string ConverterTypeName
        {
            get { return typeConverter; }
            set { typeConverter = value; }
        }
        /// <summary>
        /// Gets or sets the property's default value.
        /// </summary>
        public object DefaultValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }
        /// <summary>
        /// Gets or sets the property's description.
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// Gets or sets the type name of the property's editor.
        /// </summary>
        public string EditorTypeName
        {
            get { return editor; }
            set { editor = value; }
        }

        /// <summary>
        /// Gets or sets the property's attributes.
        /// </summary>
        public object[] EditorAttributes
        {
            get { return editorAttributes; }
            set { editorAttributes = value; }
        }
        /// <summary>
        /// Gets or sets the property's name.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets the property's type name.
        /// </summary>
        public string TypeName
        {
            get { return type; }
            set { type = value; }
        }
    }

    /// <summary>
    /// Provides data for GetValue,SetValue and ItemChanged events
    /// </summary>
    public class PropertySpecEventArgs : EventArgs
    {
        private PropertySpec property;
        private object val;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="property">Current property</param>
        /// <param name="val">Current value</param>
        public PropertySpecEventArgs(PropertySpec property, object val)
        {
            this.property = property;
            this.val = val;
        }
        /// <summary>
        /// Current property
        /// </summary>
        public PropertySpec Property
        {
            get { return property; }
        }
        /// <summary>
        /// Current value
        /// </summary>
        public object Value
        {
            get { return val; }
            set { val = value; }
        }
    }
    /// <summary>
    /// Method that handle BaseController.GetValue and BaseController.SetValue events
    /// </summary>
    public delegate void PropertySpecEventHandler(object sender, PropertySpecEventArgs e);

    /// <summary>
    /// Базовый класс для редакторов свойств
    /// </summary>
    public class BaseController : ICustomTypeDescriptor
    {
        #region PropertySpecCollection class definition
        /// <summary>
        /// Collection of PropertySpec objects
        /// </summary>
        [Serializable]
        public class PropertySpecCollection : IList
        {
            private ArrayList innerArray;

            /// <summary>
            /// Constructor
            /// </summary>
            public PropertySpecCollection()
            {
                innerArray = new ArrayList();
            }

            /// <summary>
            /// Gets the number of elements actually contained in the collection
            /// </summary>
            public int Count
            {
                get { return innerArray.Count; }
            }

            /// <summary>
            /// Gets a value indicating whether the collection has a fixed size.
            /// </summary>
            public bool IsFixedSize
            {
                get { return false; }
            }

            /// <summary>
            /// Gets a value indicating whether the collection is read only.
            /// </summary>
            public bool IsReadOnly
            {
                get { return false; }
            }

            /// <summary>
            /// Gets a value indicating whether the collection is syncronized.
            /// </summary>
            public bool IsSynchronized
            {
                get { return false; }
            }

            /// <summary>
            /// Gets or sets the element at the specified index.
            /// </summary>
            public PropertySpec this[int index]
            {
                get { return (PropertySpec)innerArray[index]; }
                set { innerArray[index] = value; }
            }

            /// <summary>
            /// Adds the property to the end of the collection
            /// </summary>
            /// <param name="value">The property to be added</param>
            /// <returns>The index at which value has been added</returns>
            public int Add(PropertySpec value)
            {
                int index = innerArray.Add(value);

                return index;
            }
            /// <summary>
            ///
            /// </summary>
            /// <param name="index"></param>
            /// <param name="value"></param>
            public void Insert(int index, PropertySpec value)
            {
                innerArray.Insert(index, value);
            }

            /// <summary>
            /// Adds the elements of the array to the end of te collection
            /// </summary>
            /// <param name="array">Array to be added</param>
            public void AddRange(PropertySpec[] array)
            {
                innerArray.AddRange(array);
            }
            /// <summary>
            /// Removes all elements from the collection
            /// </summary>
            public void Clear()
            {
                innerArray.Clear();
            }
            /// <summary>
            /// Determines whether the property is in the collection.
            /// </summary>
            /// <param name="item">The property to locate in the collection</param>
            /// <returns>true if property is found in the collection; otherwise, false</returns>
            public bool Contains(PropertySpec item)
            {
                return innerArray.Contains(item);
            }

            /// <summary>
            /// Determines whether the property with specified name is in the collection.
            /// </summary>
            /// <param name="name">The property's name to locate in the collection</param>
            /// <returns>true if property is found in the collection; otherwise, false</returns>
            public bool Contains(string name)
            {
                foreach (PropertySpec spec in innerArray)
                    if (spec.Name == name)
                        return true;

                return false;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="obj"></param>
            public void Remove(PropertySpec obj)
            {
                innerArray.Remove(obj);
            }
            /// <summary>
            ///
            /// </summary>
            /// <param name="name"></param>
            public void Remove(string name)
            {
                int index = IndexOf(name);
                RemoveAt(index);
            }
            /// <summary>
            ///
            /// </summary>
            /// <param name="index"></param>
            public void RemoveAt(int index)
            {
                innerArray.RemoveAt(index);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="array"></param>
            /// <param name="index"></param>
            public void CopyTo(PropertySpec[] array, int index)
            {
                innerArray.CopyTo(array, index);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public int IndexOf(PropertySpec value)
            {
                return innerArray.IndexOf(value);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public int IndexOf(string name)
            {
                int i = 0;

                foreach (PropertySpec spec in innerArray)
                {
                    if (spec.Name == name)
                        return i;

                    i++;
                }

                return -1;
            }


            #region Explicit interface implementations for ICollection and IList
            void ICollection.CopyTo(Array array, int index)
            {
                CopyTo((PropertySpec[])array, index);
            }

            int IList.Add(object value)
            {
                return Add((PropertySpec)value);
            }

            bool IList.Contains(object obj)
            {
                return Contains((PropertySpec)obj);
            }

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    this[index] = (PropertySpec)value;
                }
            }

            int IList.IndexOf(object obj)
            {
                return IndexOf((PropertySpec)obj);
            }

            void IList.Insert(int index, object value)
            {
                Insert(index, (PropertySpec)value);
            }

            void IList.Remove(object value)
            {
                Remove((PropertySpec)value);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return innerArray.GetEnumerator();
            }

            object ICollection.SyncRoot
            {
                get { return innerArray.SyncRoot; }
            }
            #endregion
        }
        #endregion
        #region PropertySpecDescriptor class definition
        private class PropertySpecDescriptor : PropertyDescriptor
        {
            private BaseController controller;
            private PropertySpec item;
            private string editorType = null;
            private object[] editorAttributes = null;

            public PropertySpecDescriptor(PropertySpec item, BaseController controller,
                string name, Attribute[] attrs, string EditorType, object[] EditorAttributes) :
                base(name, attrs)
            {
                this.controller = controller;
                this.item = item;
                this.editorType = EditorType;
                this.editorAttributes = EditorAttributes;
            }

            public override TypeConverter Converter
            {
                get
                {
                    foreach (object obj in Attributes)
                        if (obj is TypeConverterAttribute)
                        {
                            TypeConverterAttribute tca = obj as TypeConverterAttribute;
                            Type type = Type.GetType(tca.ConverterTypeName);
                            return (TypeConverter)Activator.CreateInstance(type);
                        }
                    return base.Converter;
                }
            }

            /*private Type FindTypeInLoadedAssemblys(string UnqTypeName)
            {
                Type res = Type.GetType(UnqTypeName);
                if (res!=null) return res;

                Assembly StartFrom = Assembly.GetEntryAssembly();
                res = StartFrom.GetType(UnqTypeName);
                if (res != null) return res;

                StartFrom = Assembly.GetExecutingAssembly();
                res = StartFrom.GetType(UnqTypeName);
                if (res != null) return res;

                return null;
            }*/

            public override object GetEditor(Type editorBaseType)
            {
                if (this.editorType != null)
                {
                    Type type = Type.GetType(this.editorType);
                    if (type != null)
                    {
                        if (this.editorAttributes != null)
                            return Activator.CreateInstance(type, new object[] { this.editorAttributes });
                        else
                            return Activator.CreateInstance(type);
                    }
                }
                return base.GetEditor(editorBaseType);
            }

            public override Type ComponentType
            {
                get { return item.GetType(); }
            }

            public override bool IsReadOnly
            {
                get { return (Attributes.Matches(ReadOnlyAttribute.Yes)); }
            }

            public override Type PropertyType
            {
                get { return Type.GetType(item.TypeName); }
            }

            public override bool CanResetValue(object component)
            {
                if (item.DefaultValue == null)
                    return false;
                else
                    return !this.GetValue(component).Equals(item.DefaultValue);
            }

            public override object GetValue(object component)
            {
                PropertySpecEventArgs e = new PropertySpecEventArgs(item, null);
                controller.OnGetValue(e);
                return e.Value;
            }

            public override void ResetValue(object component)
            {
                SetValue(component, item.DefaultValue);
            }

            public override void SetValue(object component, object value)
            {
                PropertySpecEventArgs e = new PropertySpecEventArgs(item, value);
                controller.OnSetValue(e);
            }

            public override bool ShouldSerializeValue(object component)
            {
                object val = this.GetValue(component);

                if (item.DefaultValue == null && val == null)
                    return false;
                else
                    return !val.Equals(item.DefaultValue);
            }
        }
        #endregion

        private string defaultProperty;
        private PropertySpecCollection properties;

        /// <summary>
        /// Конструктор
        /// </summary>
        public BaseController()
        {
            defaultProperty = null;
            properties = new PropertySpecCollection();
        }
        /// <summary>
        /// Specifies the default property for the object.
        /// </summary>
        public string DefaultProperty
        {
            get { return defaultProperty; }
            set { defaultProperty = value; }
        }

        /// <summary>
        /// Коллекция свойств объекта
        /// </summary>
        public PropertySpecCollection Properties
        {
            get { return properties; }
        }

        /// <summary>
        /// Появляется, когда в редакторе должно быть установлено текущее значение свойства
        /// </summary>
        public event PropertySpecEventHandler GetValue;
        /// <summary>
        /// Появляется, когда одно из полей изменено
        /// </summary>
        public event PropertySpecEventHandler SetValue;
        /// <summary>
        /// Появляется, когда значение одного из свойств изменено
        /// </summary>
        public event PropertySpecEventHandler ItemChanged;

        /// <summary>
        /// Handles the GetValue event
        /// </summary>
        protected virtual void OnGetValue(PropertySpecEventArgs e)
        {
            if (GetValue != null)
                GetValue(this, e);
        }
        /// <summary>
        /// Handles the SetValue event
        /// </summary>
        protected virtual void OnSetValue(PropertySpecEventArgs e)
        {
            if (SetValue != null)

                SetValue(this, e);
            if (ItemChanged != null)
                ItemChanged(this, e);
        }
        protected void FireItemChanged(PropertySpecEventArgs e)
        {
            if (ItemChanged != null)
                ItemChanged(this, e);
        }

        #region ICustomTypeDescriptor explicit interface definitions
        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            PropertySpec propertySpec = null;
            if (defaultProperty != null)
            {
                int index = properties.IndexOf(defaultProperty);
                propertySpec = properties[index];
            }

            if (propertySpec != null)
                return new PropertySpecDescriptor(propertySpec, this, propertySpec.Name, null,
                    propertySpec.EditorTypeName, propertySpec.EditorAttributes);
            else
                return null;
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            ArrayList props = new ArrayList();

            foreach (PropertySpec property in properties)
            {
                ArrayList attrs = new ArrayList();

                if (property.Category != null)
                    attrs.Add(new CategoryAttribute(property.Category));

                if (property.Description != null)
                    attrs.Add(new DescriptionAttribute(property.Description));

                if (property.DefaultValue != null)
                    attrs.Add(new DefaultValueAttribute(property.DefaultValue));

                if (property.EditorTypeName != null)
                    attrs.Add(new EditorAttribute(property.EditorTypeName, typeof(UITypeEditor)));

                if (property.ConverterTypeName != null)
                    attrs.Add(new TypeConverterAttribute(property.ConverterTypeName));

                if (property.ReadOnly)
                    attrs.Add(ReadOnlyAttribute.Yes);

                if (property.Attributes != null)
                    attrs.AddRange(property.Attributes);

                Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));

                PropertySpecDescriptor pd = new PropertySpecDescriptor(property,
                    this, property.Name, attrArray, property.EditorTypeName, property.EditorAttributes);
                props.Add(pd);
            }

            PropertyDescriptor[] propArray = (PropertyDescriptor[])props.ToArray(
                typeof(PropertyDescriptor));
            return new PropertyDescriptorCollection(propArray);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
        #endregion

        /// <summary>
        /// Обновление свойств перед показом
        /// </summary>
        public virtual void Update() { }

        public void AddRemovePropertySpec(bool add, params PropertySpec[] pss)
        {
            if (add)
            {
                foreach (var ps in pss)
                {
                    if (!Properties.Contains(ps))
                        Properties.Add(ps);
                }
            }
            else
            {
                foreach (var ps in pss)
                {
                    if (Properties.Contains(ps))
                        Properties.Remove(ps);
                }
            }
        }

    }

    public static class BaseControllerEx
    {
        public static T[] GetEditingObjects<T>(this ITypeDescriptorContext context) where T : class
        {
            var signleItemToEdit = context.Instance as T;
            if (signleItemToEdit != null) return new[] { signleItemToEdit };
            //return (context.Instance as T[])??new T[0];
            var objs = context.Instance as object[];
            if (objs == null) return new T[0];
            var res = new List<T>();
            foreach (object obj in objs)
            {
                var t = obj as T;
                if (t != null)
                    res.Add(t);
            }
            return res.ToArray();

        }
        public static string GetPropertyName(this ITypeDescriptorContext context)
        {
            string propName = context.ToString();
            int ix = propName.IndexOf(' ');
            if (ix < 0) throw new Exception("Can't extract property name from context");
            return propName.Substring(ix + 1);
        }
    }

    /// <summary>
    /// Редактор, позволяющий выбирать из списка
    /// </summary>
    public class ComboEditor : UITypeEditor
    {
        //private static System.Drawing.Font myFont=null;

        private IWindowsFormsEditorService edSvc = null;
        private object[] _Items;
        /// <summary>
        /// Конструктор
        /// </summary>
        public ComboEditor()
        {
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Items">Элеметы списка</param>
        public ComboEditor(object[] Items)
        {
            _Items = Items;
        }

        //static int h1,h2,h3,h4;
        /// <summary>
        /// Редактирование элемента
        /// </summary>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context != null
                && context.Instance != null
                && provider != null)
            {
                edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc != null)
                {
                    ListBox lb = new ListBox();
                    if (_Items != null)
                        lb.Items.AddRange(_Items);

                    /*
                    if (myFont==null)
                        if (CfgEditor.CfgEditorInterface.Instance!=null)
                            if (CfgEditor.CfgEditorInterface.Instance.View!=null)
                            {
                                CfgEditor.CfgEditorView View=CfgEditor.CfgEditorInterface.Instance.View;
                                if (View!=null)
                                    myFont=View._PropertyGrid.Font;
                            }
                    if (myFont!=null)
                        lb.Font=myFont;
                    */


                    lb.BorderStyle = BorderStyle.None;
                    lb.Width = 20;
                    //h1=Math.Min(lb.Items.Count*lb.ItemHeight,100);
                    //h4=lb.ItemHeight;
                    lb.Height = Math.Min(lb.Items.Count * lb.ItemHeight * 17 / 12, 100);
                    //h2=lb.Height;
                    lb.SelectedItem = value;
                    lb.SelectedIndexChanged += new EventHandler(ValueChanged);
                    // при вызове реальный ItemHeight равен не 13, а 17. В следствие чего размер ум.на четверть
                    edSvc.DropDownControl(lb);
                    //h3=lb.Height;
                    value = lb.SelectedItem;
                }
            }

            return value;
        }

        /// <summary>
        /// </summary>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context != null && context.Instance != null)
            {
                return UITypeEditorEditStyle.DropDown;
            }
            return base.GetEditStyle(context);
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            if (edSvc != null)
            {
                edSvc.CloseDropDown();
            }
        }
    }
}
