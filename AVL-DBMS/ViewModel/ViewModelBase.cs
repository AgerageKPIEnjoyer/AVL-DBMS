using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AVL_DBMS.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Подія, яка спрацьовує, коли властивість змінюється.
        /// View (UI) підписується на цю подію.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Допоміжний метод для виклику події PropertyChanged.
        /// </summary>
        /// <param name="propertyName">
        /// Ім'я властивості, що змінилася.
        /// Атрибут [CallerMemberName] автоматично підставляє
        /// ім'я методу або властивості, з якої він викликаний.
        /// </param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
