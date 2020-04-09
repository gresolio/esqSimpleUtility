using System;
using esqSimpleUtility.Tools;

namespace esqSimpleUtility.Model
{
    public class NameValuePair : IEquatable<NameValuePair>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        // To support this interesting feature:
        // "spaces before and/or after the equal-sign may be
        // entered as padding at the end-user’s discretion".
        public int PaddingLeft { get; set; }
        public int PaddingRight { get; set; }

        public override string ToString()
        {
            string eq = "=";

            if (PaddingLeft > 0)
                eq = eq.PadLeft(eq.Length + PaddingLeft);

            if (PaddingRight > 0)
                eq = eq.PadRight(eq.Length + PaddingRight);

            return $"{Name}{eq}{Value}";
        }

        public static bool TryParse(string input, out NameValuePair output)
        {
            if (!string.IsNullOrEmpty(input) && input.Contains("="))
            {
                string[] arr = input.Trim().Split('=');
                if (arr != null && arr.Length == 2)
                {
                    string name = arr[0];
                    string value = arr[1];

                    int paddingLeft = name.CountTrailingSpaces();
                    int paddingRight = value.CountLeadingSpaces();
                    name = name.Trim();
                    value = value.Trim();

                    if (name.IsAlphanumeric() && value.IsAlphanumeric())
                    {
                        output = new NameValuePair
                        {
                            Id = Guid.NewGuid(),
                            Name = name,
                            Value = value,
                            PaddingLeft = paddingLeft,
                            PaddingRight = paddingRight,
                        };
                        return true;
                    }
                }
            }

            output = null;
            return false;
        }

        public static NameValuePair CopyFrom(NameValuePair other)
        {
            return new NameValuePair
            {
                Id = other.Id,
                Name = other.Name,
                Value = other.Value,
                PaddingLeft = other.PaddingLeft,
                PaddingRight = other.PaddingRight,
            };
        }

        public bool Equals(NameValuePair other)
        {
            if (other is null)
                return false;

            // Hm... Should be Padding compared?
            return Id == other.Id
                && Name == other.Name
                && Value == other.Value;
        }
        public override bool Equals(object obj) => Equals(obj as NameValuePair);
        public override int GetHashCode() => (Id, Name, Value).GetHashCode();
    }
}
