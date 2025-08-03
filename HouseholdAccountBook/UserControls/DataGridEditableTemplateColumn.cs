using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HouseholdAccountBook.UserControls
{
    /// <summary>
    /// <see cref="DataGridTemplateColumn"/> でセル内のコントロールを編集状態にするためにクリック回数が3回必要であるのを、2回で済むようにするクラス
    /// </summary>
    /// <remarks>http://stackoverflow.com/questions/5176226/datagridtemplatecolumn-with-datepicker-requires-three-clicks-to-edit-the-date</remarks>
    public class DataGridEditableTemplateColumn : DataGridTemplateColumn
    {
        /// <remarks>セルの編集時、セル内の最初の要素にフォーカスを移動するようにする</remarks>
        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            _ = editingElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));

            return base.PrepareCellForEdit(editingElement, editingEventArgs);
        }
    }
}
