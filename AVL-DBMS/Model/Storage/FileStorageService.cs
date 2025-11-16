using System.IO;
using System.Text.Json;
using AVL_DBMS.Model.AVL_Tree_Elements;

namespace AVL_DBMS.Model.Storage
{
    public class FileStorageService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        /// <summary>
        /// Зберігає поточний стан дерева у JSON файл.
        /// </summary>
        /// <param name="tree">Екземпляр дерева для збереження.</param>
        /// <param name="filePath">Шлях до файлу (напр., "database.json").</param>
        public void SaveToFile(AVLTree tree, string filePath)
        {
            try
            {               
                List<AVLNode> nodes = tree.GetAllNodes();

                // Серіалізуємо цей список у JSON рядок                
                var dataToSave = new { Nodes = nodes };
                string jsonString = JsonSerializer.Serialize(dataToSave, _jsonOptions);

                // Записуємо JSON рядок у файл
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {                
                Console.WriteLine($"Помилка збереження у файл: {ex.Message}");                
                throw;
            }
        }

        /// <summary>
        /// Завантажує дерево з JSON файлу.
        /// </summary>
        /// <param name="filePath">Шлях до файлу (напр., "database.json").</param>
        /// <returns>Новий екземпляр AVLTree, відбудований з файлу.</returns>
        public AVLTree LoadFromFile(string filePath)
        {           
            var newTree = new AVLTree();

            try
            {                
                if (!File.Exists(filePath))
                {
                    return newTree; // Файлу немає, повертаємо порожнє дерево
                }

                // Читаємо весь JSON з файлу
                string jsonString = File.ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return newTree; // Файл порожній
                }

                // Десеріалізуємо JSON назад у список вузлів.                
                var dataLoaded = JsonSerializer.Deserialize<NodesContainer>(jsonString);

                if (dataLoaded?.Nodes == null)
                {
                    return newTree; // JSON некоректний
                }

                // Будуємо дерево заново
                foreach (var node in dataLoaded.Nodes)
                {                    
                    newTree.Insert(node.Key, node.Value);
                }

                return newTree;
            }
            catch (Exception ex)
            {
                // Якщо файл пошкоджений або JSON некоректний
                Console.WriteLine($"Помилка завантаження з файлу: {ex.Message}");
                // Повертаємо порожнє дерево, щоб програма могла стартувати
                return newTree;
            }
        }

        /// <summary>
        /// Приватний допоміжний клас-контейнер
        /// лише для коректної десеріалізації JSON.
        /// </summary>
        private class NodesContainer
        {
            public List<AVLNode> Nodes { get; set; }
        }
    }
}
