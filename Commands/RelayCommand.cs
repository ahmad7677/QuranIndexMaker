using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuranIndexMaker.Commands
{
    public class RelayCommand : ICommand
    {
        public RelayCommand(Action action)
        {
            this.action = action;
        }
        private Action action;
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            action.Invoke();
        }
    }
}
