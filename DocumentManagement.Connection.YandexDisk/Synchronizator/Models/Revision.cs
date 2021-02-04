using System;
using Newtonsoft.Json;

namespace MRS.DocumentManagement.Connection.Synchronizator
{
#pragma warning disable CS0659 // Тип переопределяет Object.Equals(object o), но не переопределяет Object.GetHashCode()
#pragma warning disable CS0661 // Тип определяет оператор == или оператор !=, но не переопределяет Object.GetHashCode()
    public class Revision : IComparable<Revision>
#pragma warning restore CS0661 // Тип определяет оператор == или оператор !=, но не переопределяет Object.GetHashCode()
#pragma warning restore CS0659 // Тип переопределяет Object.Equals(object o), но не переопределяет Object.GetHashCode()
    {
        public Revision(int id, ulong rev = 0)
        {
            ID = id;
            Rev = rev;
        }

        public int ID { get; set; }

        public ulong Rev { get; set; }

        [JsonIgnore]
        public bool IsDelete => Rev == ulong.MaxValue;

        public static bool operator >(Revision rev1, Revision rev2) => rev1.Rev > rev2.Rev;

        public static bool operator <(Revision rev1, Revision rev2) => rev1.Rev < rev2.Rev;

        public static bool operator <=(Revision left, Revision right) => ReferenceEquals(left, null) || left.CompareTo(right) <= 0;

        public static bool operator >=(Revision left, Revision right) => ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;

        public static bool operator ==(Revision left, Revision right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(Revision left, Revision right) => !(left == right);

        public void Incerment()
        {
            if (!IsDelete)
                Rev++;
        }

        public void Delete() => Rev = ulong.MaxValue;

        public override string ToString() => $"{ID}=>{Rev}";

        public int CompareTo(Revision other) => Rev.CompareTo(other.Rev);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (obj is Revision rev)
            {
                return ID == rev.ID && Rev == rev.Rev;
            }

            return false;
        }
    }
}
