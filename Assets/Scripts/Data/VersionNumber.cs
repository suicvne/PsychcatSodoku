using System;
using System.Text;

namespace IgnoreSolutions
{
    [Serializable]
    public struct VersionNumber
    {
        public short Major;
        public short Minor;
        public short Build;
        public short Revision;
        public string _Extras;

        public VersionNumber(string versionNumber)
        {
            _Extras = "";

            Major = -1;
            Minor = -1;
            Build = -1;
            Revision = -1;

            string[] numbers = versionNumber.Split(new char[] { '.', '_' });
            for (int i = 0; i < numbers.Length; i++)
            {
                short number = -1;
                if (short.TryParse(numbers[i], out number))
                {
                    switch (i)
                    {
                        case 0: Major = number; break;
                        case 1: Minor = number; break;
                        case 2: Build = number; break;
                        case 3: Revision = number; break;
                        default: _Extras += $".{number}"; break;
                    }
                }
                else
                {
                    _Extras += $";{numbers[i]}";
                }
            }
        }

        public VersionNumber(short maj, short min, short build, short rev)
        {
            Major = maj;
            Minor = min;
            Build = build;
            Revision = rev;

            _Extras = "";
        }

        public void SetVersionNumber(short maj, short min, short build, short rev)
        {
            Major = maj;
            Minor = min;
            Build = build;
            Revision = rev;

            _Extras = "";
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"{Major}.{Minor}.{Build}");
            if (Revision != -1)
                sb.Append($".{Revision}");

            return sb.ToString();
        }

        public long ToLong()
        {
            return (Major * 1000000000L +
                   Minor * 1000000L +
                   Build * 1000L +
                   Revision);
        }

        public static bool IsGreaterThan(VersionNumber a, VersionNumber b)
        {
            long lLeft = a.ToLong(), lRight = b.ToLong();
            return lLeft > lRight;
        }

        public static bool IsLessThan(VersionNumber a, VersionNumber b)
        {
            long lLeft = a.ToLong(), lRight = b.ToLong();
            return lLeft < lRight;
        }

        public static VersionNumber operator >(VersionNumber left, VersionNumber right)
        {
            long lLeft = left.ToLong(), lRight = right.ToLong();

            return lLeft > lRight ? left : right;
        }

        public static VersionNumber operator <(VersionNumber left, VersionNumber right)
        {
            long lLeft = left.ToLong(), lRight = right.ToLong();

            return lLeft < lRight ? left : right;
        }
    }
}