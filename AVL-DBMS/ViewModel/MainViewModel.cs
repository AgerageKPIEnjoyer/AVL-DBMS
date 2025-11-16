using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AVL_DBMS.Model.AVL_Tree_Elements;
using AVL_DBMS.Model.Storage;

namespace AVL_DBMS.ViewModel
{
    public class MainViewModel : ViewModelBase
    {       
        private AVLTree _databaseTree;                
        private readonly FileStorageService _storageService;
        private readonly string _databaseFolderPath;

        private const int MaxKeyCharacterLength = 6;

        private string _keyInput;
        /// <summary>
        /// Прив'язується до TextBox для введення Ключа.
        /// </summary>
        public string KeyInput
        {
            get => _keyInput;
            set
            {
                _keyInput = value;
                OnPropertyChanged();
            }
        }

        private string _valueInput;
        /// <summary>
        /// Прив'язується до TextBox для введення Даних.
        /// </summary>
        public string ValueInput
        {
            get => _valueInput;
            set
            {
                _valueInput = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        /// <summary>
        /// Прив'язується до TextBlock для виводу результатів та помилок.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        private string _generationCountInput;
        /// <summary>
        /// Прив'язується до TextBox для введення кількості записів.
        /// </summary>
        public string GenerationCountInput
        {
            get => _generationCountInput;
            set
            {
                _generationCountInput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Публічна властивість, що показує поточну к-сть вузлів.
        /// </summary>
        public int CurrentTreeCount => _databaseTree.Count;

        /// <summary>
        /// Список файлів БД для ComboBox
        /// </summary>
        public ObservableCollection<string> DatabaseFiles { get; set; }

        private string _selectedDatabaseFile;
        /// <summary>
        /// Файл, обраний у ComboBox.
        /// </summary>
        public string SelectedDatabaseFile
        {
            get => _selectedDatabaseFile;
            set
            {
                _selectedDatabaseFile = value;
                OnPropertyChanged();

                // АВТОМАТИЧНО завантажуємо БД, коли користувач щось обрав
                if (!string.IsNullOrEmpty(_selectedDatabaseFile))
                {
                    ExecuteLoadDatabase(_selectedDatabaseFile);
                }
            }
        }

        private string _databaseNameInput;
        /// <summary>
        /// Ім'я файлу, яке користувач вводить для збереження.
        /// </summary>
        public string DatabaseNameInput
        {
            get => _databaseNameInput;
            set
            {
                _databaseNameInput = value;
                OnPropertyChanged();
            }
        }
        public ICommand AddCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand GenerateDataCommand { get; }
        public ICommand NewDatabaseCommand { get; }        
        public ICommand SaveDatabaseCommand { get; }
        public ICommand DeleteDatabaseCommand { get; }

        public event Action TreeChanged;      

        public MainViewModel()
        {            
            _storageService = new FileStorageService();
            _databaseFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Databases");

            // Створюємо папку, якщо її не існує
            Directory.CreateDirectory(_databaseFolderPath);
            _databaseTree = new AVLTree();
            StatusMessage = "Готово до роботи. Створено нову (порожню) базу даних.";
            DatabaseFiles = new ObservableCollection<string>();

            AddCommand = new RelayCommand(ExecuteAdd);
            SearchCommand = new RelayCommand(ExecuteSearch);
            EditCommand = new RelayCommand(ExecuteEdit);
            RemoveCommand = new RelayCommand(ExecuteRemove);
            GenerateDataCommand = new RelayCommand(ExecuteGenerateData);

            NewDatabaseCommand = new RelayCommand(ExecuteNewDatabase);
            SaveDatabaseCommand = new RelayCommand(ExecuteSaveDatabase);
            DeleteDatabaseCommand = new RelayCommand(ExecuteDeleteDatabase);
            LoadDatabaseList();
        }

        /// <summary>
        /// Сканує папку 'Databases' і оновлює список файлів (ComboBox).
        /// </summary>
        private void LoadDatabaseList()
        {
            DatabaseFiles.Clear();
            var files = Directory.GetFiles(_databaseFolderPath, "*.json")
                                 .Select(Path.GetFileName); // Отримуємо лише імена

            foreach (var file in files)
            {
                DatabaseFiles.Add(file);
            }
        }        

        private void ExecuteNewDatabase(object parameter)
        {
            _databaseTree = new AVLTree();
            StatusMessage = "Створено нову порожню базу даних.";
            DatabaseNameInput = string.Empty; // Очищуємо поле
            SelectedDatabaseFile = null; // Скидаємо вибір у ComboBox

            OnPropertyChanged(nameof(CurrentTreeCount));
            TreeChanged?.Invoke();
        }

        /// <summary>
        /// Завантажує БД (викликається зі 'set' SelectedDatabaseFile).
        /// </summary>
        private void ExecuteLoadDatabase(string fileName)
        {
            string filePath = Path.Combine(_databaseFolderPath, fileName);
            try
            {
                _databaseTree = _storageService.LoadFromFile(filePath);
                StatusMessage = $"Базу даних успішно завантажено з: {fileName}";

                // Оновлюємо поле 'Зберегти' ім'ям завантаженого файлу
                DatabaseNameInput = Path.GetFileNameWithoutExtension(fileName);

                OnPropertyChanged(nameof(CurrentTreeCount));
                TreeChanged?.Invoke();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Помилка завантаження файлу: {ex.Message}";
                DatabaseNameInput = string.Empty;
            }
        }

        /// <summary>
        /// Зберігає поточну БД у папку 'Databases'
        /// з іменем з 'DatabaseNameInput'.
        /// </summary>
        private void ExecuteSaveDatabase(object parameter)
        {
            // Валідація імені файлу
            if (string.IsNullOrWhiteSpace(DatabaseNameInput))
            {
                StatusMessage = "Помилка: Введіть ім'я файлу для збереження.";
                return;
            }

            // Запобігаємо недійсним символам у назві файлу
            if (DatabaseNameInput.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                StatusMessage = "Помилка: Ім'я файлу містить недійсні символи.";
                return;
            }

            // Збереження
            string fileName = DatabaseNameInput + ".json";
            string filePath = Path.Combine(_databaseFolderPath, fileName);

            try
            {
                _storageService.SaveToFile(_databaseTree, filePath);
                StatusMessage = $"Базу даних успішно збережено у: {fileName}";

                // Оновлення списку файлів (ComboBox)
                LoadDatabaseList();
                // Встановлюємо збережений файл як обраний
                _selectedDatabaseFile = fileName;
                OnPropertyChanged(nameof(SelectedDatabaseFile));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Помилка збереження файлу: {ex.Message}";
            }
        }

        /// <summary>
        /// Видаляє обраний файл бази даних з диска.
        /// </summary>
        private void ExecuteDeleteDatabase(object parameter)
        {
            string fileToDelete = SelectedDatabaseFile;

            // Перевірка, чи щось обрано
            if (string.IsNullOrEmpty(fileToDelete))
            {
                StatusMessage = "Помилка: Файл для видалення не обрано.";
                return;
            }

            // Діалог підтвердження
            MessageBoxResult result = MessageBox.Show(
                $"Ви дійсно хочете назавжди видалити файл '{fileToDelete}'?",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {
                StatusMessage = "Видалення скасовано.";
                return;
            }

            // Видалення
            try
            {
                string filePath = Path.Combine(_databaseFolderPath, fileToDelete);

                // Фізичне видалення файлу
                File.Delete(filePath);

                StatusMessage = $"Файл '{fileToDelete}' успішно видалено.";
                                
                // Оновлюємо список файлів у ComboBox
                LoadDatabaseList();

                // Скидаємо поточну сесію (створюємо нову порожню БД)
                // Це автоматично очистить дерево, лічильники і SelectedDatabaseFile
                ExecuteNewDatabase(null);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Помилка видалення файлу: {ex.Message}";
            }
        }

        /// <summary>
        /// Надає корінь дерева для візуалізатора.
        /// </summary>
        public AVLNode GetTreeRoot()
        {
            return _databaseTree.GetRootNode();
        }

        /// <summary>
        /// Парсить KeyInput і встановлює StatusMessage у разі помилки.
        /// </summary>
        /// <param name="key">Вихідний параметр для розпарсеного ключа.</param>
        /// <returns>true, якщо валідація успішна, інакше false.</returns>
        private bool ValidateAndParseKey(out int key)
        {
            if (string.IsNullOrWhiteSpace(KeyInput))
            {
                StatusMessage = "Помилка: Поле 'Ключ' не може бути порожнім.";
                key = 0;
                return false;
            }

            if (KeyInput.Length > MaxKeyCharacterLength)
            {
                StatusMessage = $"Помилка: Ключ не може бути довшим за {MaxKeyCharacterLength} символів.";
                key = 0;
                return false;
            }

            if (!int.TryParse(KeyInput, out key))
            {
                StatusMessage = "Помилка: 'Ключ' має бути цілим числом.";
                return false;
            }

            return true;
        }   

        private void ExecuteAdd(object parameter)
        {
            if (!ValidateAndParseKey(out int key))
            {
                return; 
            }

            if (string.IsNullOrEmpty(ValueInput))
            {
                StatusMessage = "Помилка: Поле 'Дані' не може бути порожнім при додаванні.";
                return;
            }

            try
            {
                bool success = _databaseTree.Insert(key, ValueInput);

                if (success)
                {
                    StatusMessage = $"Запис з ключем {key} успішно додано.";
                    OnPropertyChanged(nameof(CurrentTreeCount));
                    TreeChanged?.Invoke();
                }
                else
                {
                    StatusMessage = $"Помилка: Запис з ключем {key} вже існує.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Помилка додавання: {ex.Message}";
            }
        }

        private void ExecuteSearch(object parameter)
        {
            if (!ValidateAndParseKey(out int key))
            {
                return;
            }

            try
            {
                var (found, value, comparisons) = _databaseTree.Search(key);

                if (found)
                {
                    ValueInput = value; // Показуємо дані (навіть якщо вони null/empty)
                    StatusMessage = $"Знайдено запис. Ключ: {key}. Кількість порівнянь: {comparisons}.";
                }
                else
                {
                    StatusMessage = $"Ключ {key} не знайдено. Кількість порівнянь: {comparisons}.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Помилка пошуку: {ex.Message}";
            }
        }

        private void ExecuteEdit(object parameter)
        {
            if (!ValidateAndParseKey(out int key))
            {
                return;
            }

            try
            {
                bool success = _databaseTree.Edit(key, ValueInput);

                if (success)
                {
                    StatusMessage = $"Запис {key} успішно оновлено.";                    
                    TreeChanged?.Invoke();
                }
                else
                {
                    StatusMessage = $"Помилка оновлення: Ключ {key} не знайдено.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Помилка редагування: {ex.Message}";
            }
        }

        private void ExecuteRemove(object parameter)
        {
            if (!ValidateAndParseKey(out int key))
            {
                return;
            }

            try
            {
                bool success = _databaseTree.Remove(key);
                if (success)
                {
                    StatusMessage = $"Запис з ключем {key} успішно видалено.";                   
                    OnPropertyChanged(nameof(CurrentTreeCount));
                    TreeChanged?.Invoke();
                }
                else
                {
                    StatusMessage = $"Помилка: Запис з ключем {key} не знайдено.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Помилка видалення: {ex.Message}";
            }
        }

        /// <summary>
        /// Генерує задану кількість записів з послідовними ключами.
        /// </summary>
        private void ExecuteGenerateData(object parameter)
        {
            // Валідація вхідних даних 
            if (!int.TryParse(GenerationCountInput, out int count) || count <= 0)
            {
                StatusMessage = "Помилка: Кількість записів має бути додатнім числом.";
                return;
            }

            if (count > 50000)
            {
                StatusMessage = "Помилка: Максимальна кількість записів - 50,000.";
                return;
            }

            try
            {
                StatusMessage = $"Генерація {count} записів...";

                // Очищуємо поточне дерево
                _databaseTree = new AVLTree();
                var rand = new Random();

                // Генеруємо дані
                for (int i = 1; i <= count; i++)
                {
                    // Послідовні ключі, випадкові значення
                    string randomValue = GenerateRandomString(10, rand);
                    _databaseTree.Insert(i, randomValue);
                }

                StatusMessage = $"Успішно згенеровано {count} записів.";

                // Сповіщуємо UI
                OnPropertyChanged(nameof(CurrentTreeCount)); // Оновлюємо лічильник
                TreeChanged?.Invoke(); // Запускаємо перемальовування (View вирішить, чи малювати)
            }
            catch (Exception ex)
            {
                StatusMessage = $"Помилка генерації: {ex.Message}";
            }
        }

        private string GenerateRandomString(int length, Random random)
        {            
            StringBuilder value = new StringBuilder(length);          
            for (int i = 0; i < length; i++)
            {
                char symbol;
                do
                {
                    symbol = Convert.ToChar(random.Next(32, 127));
                } while (symbol == ' ');
                value.Append(symbol);
            }
            return value.ToString();
        }         
    }
}