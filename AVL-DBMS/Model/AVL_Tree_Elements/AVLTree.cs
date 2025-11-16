namespace AVL_DBMS.Model.AVL_Tree_Elements
{
    public class AVLTree
    {
        private AVLNode _root;
        public int Count { get; private set; }
        public AVLTree()
        {
            _root = null;
            Count = 0;
        }
        public AVLNode GetRootNode()
        {
            return _root;
        }        

        public (bool Found, string Value, int Comparisons) Search(int key)
        {            
            return SearchNode(_root, key, 1);
        }

        /// <summary>
        /// Вставляє новий запис у дерево та виконує балансування.
        /// </summary>
        public bool Insert(int key, string value)
        {
            int oldCount = Count;
            _root = InsertNode(_root, key, value);
            return Count > oldCount;
        }
        public bool Edit(int key, string newValue)
        {
            AVLNode current = _root;

            while (current != null)
            {
                if (key == current.Key) // Знайдено бажаний вузол
                {
                    current.Value = newValue;
                    return true;
                }
                else if (key < current.Key)
                {
                    // Йдемо вліво
                    current = current.Left;
                }
                else
                {
                    // Йдемо вправо
                    current = current.Right;
                }
            }

            
            // Вузол не знайдено.
            return false;
        }

        /// <summary>
        /// Видаляє запис за ключем та виконує балансування.
        /// </summary>
        public bool Remove(int key)
        {
            int oldCount = Count;
            _root = RemoveNode(_root, key);
            return Count < oldCount;
        }
         
        /// <summary>
        /// Отримує висоту вузла (повертає 0 для null-вузлів).
        /// </summary>
        private int GetHeight(AVLNode node)
        {
            return node?.Height ?? 0;
        }

        /// <summary>
        /// Розраховує фактор балансування для вузла.
        /// </summary>
        private int GetBalance(AVLNode node)
        {
            if (node == null)
                return 0;

            // Висота правого - висота лівого
            return GetHeight(node.Left) - GetHeight(node.Right);
        }

        /// <summary>
        /// Оновлює висоту вузла на основі висот його нащадків.
        /// </summary>
        private void UpdateHeight(AVLNode node)
        {
            if (node != null)
            {
                node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
            }
        }         

        private AVLNode RotateLeft(AVLNode x)
        {
            // Отримуємо 'y' (правий нащадок 'x')
            AVLNode y = x.Right;

            // Отримуємо 'T2' (лівий нащадок 'y')
            AVLNode T2 = y.Left;

            // Виконуємо поворот: 'y' стає новим коренем
            y.Left = x;

            // 'T2' стає правим нащадком 'x'
            x.Right = T2;

            // Оновлюємо висоти. Спочатку оновлюємо 'x' (тепер він нижче),
            //    а потім 'y' (новий корінь).
            UpdateHeight(x);
            UpdateHeight(y);

            // Повертаємо 'y' як новий корінь цього піддерева
            return y;
        }
        private AVLNode RotateRight(AVLNode y)
        {
            // Отримуємо 'x' (лівий нащадок 'y')
            AVLNode x = y.Left;

            // Отримуємо 'T2' (правий нащадок 'x')
            AVLNode T2 = x.Right;

            // Виконуємо поворот: 'x' стає новим коренем
            x.Right = y;

            // 'T2' стає лівим нащадком 'y'
            y.Left = T2;

            // Оновлюємо висоти. Спочатку 'y' (нижній), потім 'x' (верхній).
            UpdateHeight(y);
            UpdateHeight(x);

            // Повертаємо 'x' як новий корінь
            return x;
        }       

        private AVLNode InsertNode(AVLNode node, int key, string value)
        {          
            // Якщо ми досягли порожньої гілки, створюємо і повертаємо новий вузол
            if (node == null)
            {
                Count++;
                return new AVLNode(key, value);
            }

            // Йдемо рекурсивно вліво чи вправо
            if (key < node.Key)
            {
                node.Left = InsertNode(node.Left, key, value);
            }
            else if (key > node.Key)
            {
                node.Right = InsertNode(node.Right, key, value);
            }
            else
            {                
                return node;
            }
            
            // Оновлюємо висоту поточного вузла пісоя вставки           
            UpdateHeight(node);
                        
            // Отримуємо баланс, щоб перевірити, чи не порушився він.
            int balance = GetBalance(node);    
            
            // Перевіряємо 4 випадки дисбалансу
            // Випадок 1: Left-Left (LL) - "Ліво-важкий"            
            if (balance > 1 && key < node.Left.Key)
            {
                return RotateRight(node);
            }

            // Випадок 2: Right-Right (RR) - "Право-важкий"            
            if (balance < -1 && key > node.Right.Key)
            {
                return RotateLeft(node);
            }

            // Випадок 3: Left-Right (LR) - "Ліво-важкий"            
            if (balance > 1 && key > node.Left.Key)
            {
                node.Left = RotateLeft(node.Left);
                return RotateRight(node);
            }

            // Випадок 4: Right-Left (RL) - "Право-важкий"            
            if (balance < -1 && key < node.Right.Key)
            {
                node.Right = RotateRight(node.Right);
                return RotateLeft(node);
            }

            // Якщо баланс не порушено, просто повертаємо незмінений вузол
            return node;
        }       

        private (bool Found, string Value, int Comparisons) SearchNode(AVLNode node, int key, int comparisons)
        {
            // Збільшуємо лічильник порівнянь кожного разу,
            // коли ми "відвідуємо" вузол (включаючи null).
            comparisons++;

            // Випадок 1: Вузол не знайдено (дійшли до кінця гілки)
            if (node == null)
            {
                return (false, null, comparisons);
            }

            // Випадок 2: Ключ знайдено
            if (key == node.Key)
            {
                return (true, node.Value, comparisons);
            }

            comparisons++;

            // Випадок 3: Ключ менший, йдемо вліво
            if (key < node.Key)
            {
                return SearchNode(node.Left, key, comparisons);
            }

            // Випадок 4: Ключ більший, йдемо вправо
            else 
            {
                return SearchNode(node.Right, key, comparisons);
            }
        }
        private AVLNode FindMinNode(AVLNode node)
        {
            AVLNode current = node;
            // Йдемо максимально вліво
            while (current.Left != null)
            {
                current = current.Left;
            }
            return current;
        }
        private AVLNode FindMaxNode(AVLNode node)
        {
            AVLNode current = node;
            // Йдемо максимально вправо
            while (current.Right != null)
            {
                current = current.Right;
            }
            return current;
        }

        private AVLNode RemoveNode(AVLNode node, int key)
        {
            
            if (node == null) // Вузол не знайдено
            {
                return node; 
            }

            // Пошук вузла
            if (key < node.Key)
            {
                node.Left = RemoveNode(node.Left, key);
            }
            else if (key > node.Key)
            {
                node.Right = RemoveNode(node.Right, key);
            }
            else // Знайдено вузол для видалення
            {
                // Випадок 1: 0 або 1 нащадок
                if (node.Left == null || node.Right == null)
                {
                    AVLNode temp = node.Left != null ? node.Left : node.Right;
                    if (temp == null) // 0 нащадків
                    {
                        node = null;
                    }
                    else // 1 нащадок
                    {
                        node = temp; // Замінюємо вузол нащадком
                    }

                    Count--;
                }
                else // Випадок 2: Вузол має 2 нащадків
                {                  

                    int currentBalance = GetBalance(node); // Отримуємо поточний баланс вузла
                                       
                    // Беремо найбільший вузол з лівого піддерева 
                    if (currentBalance > 0)
                    {
                        AVLNode temp = FindMaxNode(node.Left);

                        // Копіюємо дані
                        node.Key = temp.Key;
                        node.Value = temp.Value;

                        // Рекурсивно видаляємо цей вузол з лівого піддерева
                        node.Left = RemoveNode(node.Left, temp.Key);
                    }
                    // Якщо праве піддерево важче АБО баланс = 0 
                    else 
                    {
                        // Знаходимо найменший вузол у правому піддереві (successor)
                        AVLNode temp = FindMinNode(node.Right);

                        // Копіюємо дані
                        node.Key = temp.Key;
                        node.Value = temp.Value;

                        // Рекурсивно видаляємо цей вузол з правого піддерева
                        node.Right = RemoveNode(node.Right, temp.Key);
                    }
                }
            }

            // Якщо дерево стало порожнім (видалили єдиний вузол)
            if (node == null)
            {
                return node;
            }

            // Оновлення висоти ---
            UpdateHeight(node);
                        
            int balance = GetBalance(node);            

            // Випадок 1: Left-Left (LL) - "Ліво-важкий"           
            if (balance > 1 && GetBalance(node.Left) >= 0)
            {
                return RotateRight(node);
            }

            // Випадок 2: Right-Right (RR) - "Право-важкий"            
            if (balance < -1 && GetBalance(node.Right) <= 0)
            {
                return RotateLeft(node);
            }

            // Випадок 3: Left-Right (LR) - "Ліво-важкий"            
            if (balance > 1 && GetBalance(node.Left) < 0)
            {
                node.Left = RotateLeft(node.Left);
                return RotateRight(node);
            }

            // Випадок 4: Right-Left (RL) - "Право-важкий"           
            if (balance < -1 && GetBalance(node.Right) > 0)
            {
                node.Right = RotateRight(node.Right);
                return RotateLeft(node);
            }

            // Повертаємо вузол (можливо, новий корінь піддерева після поворотів)
            return node;
        }

        /// <summary>
        /// Публічний метод для отримання всіх вузлів у вигляді списку.
        /// Це потрібно для серіалізації (збереження у файл).
        /// </summary>
        public List<AVLNode> GetAllNodes()
        {
            var nodes = new List<AVLNode>();           
            GetAllNodesRecursive(_root, nodes);
            return nodes;
        }

        /// <summary>
        /// Приватний рекурсивний хелпер для збору всіх вузлів у список.
        /// </summary>
        private void GetAllNodesRecursive(AVLNode node, List<AVLNode> nodes)
        {
            if (node != null)
            {               
                nodes.Add(node);                
                GetAllNodesRecursive(node.Left, nodes);                
                GetAllNodesRecursive(node.Right, nodes);
            }
        }
    }
}
