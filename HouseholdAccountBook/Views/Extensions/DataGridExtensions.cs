using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace HouseholdAccountBook.Views.Extensions
{
    public static class DataGridExtensions
    {
        /// <summary>
        /// DateGrid内の表示文字列のリストを取得する
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<string>> ExtractDisplayValues(this DataGrid dataGrid)
        {
            List<List<string>> result = [];

            List<DataGridBoundColumn> columns = [.. dataGrid.Columns
                .Where(c => c.Visibility == Visibility.Visible)
                .OrderBy(c => c.DisplayIndex)
                .OfType<DataGridBoundColumn>()];

            foreach (object item in dataGrid.Items) {
                if (item == CollectionView.NewItemPlaceholder) {
                    continue;
                }

                List<string> row = [];

                foreach (var column in columns) {
                    if (column.Binding is not Binding binding) {
                        row.Add(string.Empty);
                        continue;
                    }

                    object value = BindingEvaluator.Evaluate(item, binding);
                    row.Add(value?.ToString() ?? string.Empty);
                }

                result.Add(row);
            }

            return result;
        }
    }
}
