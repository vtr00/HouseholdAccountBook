namespace HouseholdAccountBook.Interfaces
{
    /// <summary>
    /// 複数選択が可能なコントロールにバインドするViewModelのためのインターフェイス
    /// </summary>
    /// <example>
    /// 伝達経路：
    ///   VM.SelectedItemVM<->Behavior.SelectedItem<->VM.IsSelected<->View.IsSelected
    /// 
    /// <![CDATA[
    /// <TreeViewItemsSource="{Binding HierachicalItemVMList}">
    ///     <TreeView.ItemContainerStyle>
    ///         <Style TargetType="TreeViewItem">
    ///             <Setter Property="IsSelected" Value="{Binding IsSelected}" />
    ///         </Style>
    ///     </TreeView.ItemContainerStyle>
    ///     <i:Interaction.Behaviors>
    ///         <behaviors:BindSelectedItemToTreeViewBehavior SelectedItem="{Binding SelectedItemVM}" />
    ///     </i:Interaction.Behaviors>
    /// </TreeView>
    /// ]]>
    /// </example>
    public interface IMultiSelectable
    {
        /// <summary>
        /// 選択されているか
        /// </summary>
        #region IsSelected
        bool IsSelected { get; set; }
        #endregion
    }
}
