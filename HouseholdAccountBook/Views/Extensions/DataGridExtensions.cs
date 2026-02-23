using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace HouseholdAccountBook.Views.Extensions
{
    public static class DataGridExtensions
    {
        /// <summary>
        /// <see cref="DataGrid"/> の列情報
        /// </summary>
        public class ColumnInfo
        {
            /// <summary>
            /// <see cref="Binding.Path"/>
            /// </summary>
            public string PropertyPath { get; set; }
            /// <summary>
            /// <see cref="DataGridColumn.Header"/>
            /// </summary>
            public string HeaderText { get; set; }
            /// <summary>
            /// <see cref="DataGridColumn.DisplayIndex"/>
            /// </summary>
            public int DisplayIndex { get; set; }
            /// <summary>
            /// <see cref="BindingBase.StringFormat"/>
            /// </summary>
            public string StringFormat { get; set; }
            /// <summary>
            /// <see cref="Binding.Converter"/>
            /// </summary>
            public IValueConverter Converter { get; set; }
            /// <summary>
            /// <see cref="Binding.ConverterParameter"/>
            /// </summary>
            public object ConverterParameter { get; set; }
            /// <summary>
            /// <see cref="Binding.ConverterCulture"/>
            /// </summary>
            public CultureInfo ConverterCulture { get; set; }
        }

        /// <summary>
        /// <see cref="DataGrid"/> の列情報を取得する
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <returns>列情報</returns>
        public static IEnumerable<ColumnInfo> GetColumnInfo(this DataGrid dataGrid)
        {
            return [.. dataGrid.Columns
                .Where(c => c.Visibility == Visibility.Visible)
                .OrderBy(c => c.DisplayIndex)
                .Select(c => {
                    ColumnInfo info = new () {
                        HeaderText = c.Header?.ToString(),
                        DisplayIndex = c.DisplayIndex
                    };

                    if (c is DataGridBoundColumn boundColumn &&
                        boundColumn.Binding is Binding binding) {
                        info.PropertyPath = binding.Path?.Path;
                        info.StringFormat = binding.StringFormat;
                        info.Converter = binding.Converter;
                        info.ConverterParameter = binding.ConverterParameter;
                        info.ConverterCulture = binding.ConverterCulture;
                    }

                    return info;
                })];
        }
    }
}
