namespace PcInputX
{

    public class MenuPoint
    {
        public int Value { get; private set; }

        public static int Record = 0;
        public static int Play = 1;
        public static int PlayInLoop = 2;
        public static int Exit = 3;

        private MenuPoint(int value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            var otherObj = obj as MenuPoint;
            return otherObj != null && Value.Equals(otherObj.Value);
        }

        protected bool Equals(MenuPoint other) => Value == other.Value;

        public override int GetHashCode() => Value;

        public static implicit operator MenuPoint(int otherType) => new MenuPoint(otherType);

        public static MenuPoint operator++(MenuPoint menuPoint)
        {
            menuPoint.Value = (menuPoint.Value + 1) % Exit;
            return menuPoint;
        }

        public static MenuPoint operator--(MenuPoint menuPoint)
        {
            menuPoint.Value = (menuPoint.Value - 1) % Exit;
            return menuPoint;
        }
    }
}
