using System;
//using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using IndEditorDialogs;

namespace Configurator.ViewModel
{
    public class IndicatorEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            string res = EditExpression( (value as string) ?? "");
            //string res = IndEditorDialogs.IndEditorDlg.Edit(IndEditorDialogs.IndEditorDlgExpressionType.Indicator, (value as string) ?? "");
            if (res != null) return res;
            return value;
        }
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        private static string EditExpression( string expression)
        {

            //var instrumentsList = data.GetMfInstrumentNames();
            return IndEditorDlg.EditWithoutInstrument(IndEditorDlgExpressionType.Indicator, expression, null,
                new IndEditorDlg.Options
                {
                    //Title = title,
                    HasTickData = false,
                    AllowRenkoBars = false,
                    //InjectedControls =
                    //    new List<IndEditorDlg.IInjectedControl> { new StrategyGroupsControl(data.TradingConfiguration) },
                    //Validating = (i, t, e, s) => ValidateWithNotEmptyStrategyGroups(i, t, e, s, instrumentsList)
                });
        }

    }
}