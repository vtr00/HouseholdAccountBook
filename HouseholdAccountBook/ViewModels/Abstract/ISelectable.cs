namespace HouseholdAccountBook.ViewModels.Abstract
{
    /// <summary>
    /// 項目選択が可能なコントロールにバインドするViewModelのためのインターフェイス
    /// </summary>
    /// <example>
    /// 伝達経路：
    ///   VM.SelectedVM - Behavior.SelectedItem - VM.SelectFlag - Item.IsSelected
    /// 
    /// <![CDATA[
    /// <TreeView ItemsSource="{Binding VMList}">
    ///     <i:Interaction.Behaviors>
    ///         <behaviors:BindSelectedItemToTreeViewBehavior SelectedItem="{Binding SelectedVM}" />
    ///     </i:Interaction.Behaviors>
    ///     <TreeView.ItemContainerStyle>
    ///         <Style TargetType="TreeViewItem">
    ///             <Setter Property="IsSelected" Value="{Binding SelectFlag}" />
    ///         </Style>
    ///     </TreeView.ItemContainerStyle>
    /// </TreeView>
    /// ]]>
    /// </example>
    public interface ISelectable
    {
        /// <summary>
        /// 選択されているか
        /// </summary>
        #region SelectFlag
        bool SelectFlag { get; set; }
        #endregion
    }
}
