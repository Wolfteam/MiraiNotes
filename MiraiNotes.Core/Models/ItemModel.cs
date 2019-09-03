namespace MiraiNotes.Core.Models
{
    public class ItemModel
    {
        public string ItemId { get; set; }
        public string Text { get; set; }

        //TODO: CHECK IF THIS CHANGES DOES NOT BREAK UWP

        public override string ToString()
        {
            return Text;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as ItemModel;
            if (rhs == null)
                return false;
            return rhs.Text == Text;
        }

        public override int GetHashCode()
        {
            if (Text == null)
                return 0;
            return Text.GetHashCode();
        }
    }
}
