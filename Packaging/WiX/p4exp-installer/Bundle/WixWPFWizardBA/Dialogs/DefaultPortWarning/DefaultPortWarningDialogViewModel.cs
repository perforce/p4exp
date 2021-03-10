using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WixWPFWizardBA.Views;
using System.ComponentModel; // INotifyPropertyChanged, PropertyChangedEventHandler, PropertyChangedEventArgs
using System.Linq.Expressions;

namespace WixWPFWizardBA.Dialogs.DefaultPortWarning
{
    public class DefaultPortWarningDialogViewModel : ViewModelBase
    {
        private string _dialogTitle;

        public string DialogTitle
        {
            get
            {
                return _dialogTitle;
            }
            set
            {
                _dialogTitle = value;
                OnPropertyChanged("DialogTitle"); // This results in it being displayed.
            }
        }
    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        public ViewModelBase()
        {

        }

        protected virtual void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = propertyExpression.Body as MemberExpression;
                string propertyName = memberExpr.Member.Name;
                this.OnPropertyChanged(propertyName);
            }
        }

    }

}
