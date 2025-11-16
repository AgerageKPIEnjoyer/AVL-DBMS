namespace AVL_DBMS.Model.AVL_Tree_Elements
{
    public class AVLNode
    {        
        public int Key { get; set; }        
        public string Value { get; set; }

        /// Висота піддерева, коренем якого є цей вузол.
        /// (Лист має висоту 1)
        public int Height { get; set; }        
        public AVLNode? Left { get; set; }        
        public AVLNode? Right { get; set; }
        public AVLNode(int key, string value)
        {
            Key = key;
            Value = value;
            Height = 1; // Початкова висота нового вузла (листа)
            Left = null;
            Right = null;
        }
    }
}
