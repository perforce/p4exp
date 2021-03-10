using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace WixWPFWizardBA.Dialogs.FolderBrowser
{
    public static class TreeViewItemBehaviour
    {
        #region IsBroughtIntoViewWhenSelected

        public static bool GetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem)
        {
            return (bool)treeViewItem.GetValue(IsBroughtIntoViewWhenSelectedProperty);
        }

        public static void SetIsBroughtIntoViewWhenSelected(
          TreeViewItem treeViewItem, bool value)
        {
            treeViewItem.SetValue(IsBroughtIntoViewWhenSelectedProperty, value);
        }

        public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
            DependencyProperty.RegisterAttached(
            "IsBroughtIntoViewWhenSelected",
            typeof(bool),
            typeof(TreeViewItemBehaviour),
            new UIPropertyMetadata(false, OnIsBroughtIntoViewWhenSelectedChanged));

        static void OnIsBroughtIntoViewWhenSelectedChanged(
          DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem item = depObj as TreeViewItem;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
            {
                item.Selected += item_Selected;
            }
            else
            {
                item.Selected -= item_Selected;
            }
        }

        static void item_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.OriginalSource as TreeViewItem;
            if (item != null)
                item.BringIntoView();
        }

        #endregion // IsBroughtIntoViewWhenSelected

    }
}
