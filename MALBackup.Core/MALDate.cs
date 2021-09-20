using System;
using System.Text.RegularExpressions;

namespace MALBackup.Core
{
    #pragma warning disable CS0661
    #pragma warning disable CS0660

    public struct MALDate

    #pragma warning restore CS0660
    #pragma warning restore CS0661
    {
        public readonly int? Day { get; }

        public readonly int? Month { get; }

        public readonly int Year { get; }

        /// <summary>
        /// Date format must be "mm-dd-yy", "mm-00-yy" or "00-00-yy".
        /// </summary>
        public MALDate( string date )
        {
            var regex = new Regex( @"^(0\d|1[0-2])-(0\d|[12]\d|3[01])-(\d{2})$" );
            Match match = regex.Match( date );

            if( !match.Success )
            {
                throw new FormatException( $"Invalid date format: {date}" );
            }

            int day = int.Parse( match.Groups[2].Value );
            int month = int.Parse( match.Groups[1].Value );
            int year = int.Parse( match.Groups[3].Value );

            if( day > 0 && month == 0 )
            {
                throw new FormatException( $"Invalid date format: {date}" );
            }

            Day = day == 0 ? null : day;
            Month = month == 0 ? null : month;

            // Warning, ambigious values are possible: the year 17 of 1917 will be set as 2017
            Year = year + (year > new DateTime().Year + 2 ? 1900 : 2000);
        }

        public MALDate( DateTime dateTime )
        {
            Day = dateTime.Day;
            Month = dateTime.Month;
            Year = dateTime.Year;
        }

        public override string ToString()
        {
            string day = Day is null ? "00" : Day < 10 ? "0" + Day.ToString() : Day.ToString();
            string month = Month is null ? "00" : Month < 10 ? "0" + Month.ToString() : Month.ToString();
            return $"{month}-{day}-{Year.ToString().Substring( 2, 2 )}";
        }

        public static bool operator ==( MALDate date1, MALDate date2 ) =>
            date1.Day == date2.Day &&
            date1.Month == date2.Month &&
            date1.Year == date2.Year;

        public static bool operator !=( MALDate date1, MALDate date2 ) =>
            date1.Day != date2.Day ||
            date1.Month != date2.Month ||
            date1.Year != date2.Year;

        public static bool operator <( MALDate date1, MALDate date2 ) =>
            date1.Year < date2.Year ||
            (date1.Year == date2.Year && date1.Month < date2.Month) ||
            (date1.Year == date2.Year && date1.Month == date2.Month && date1.Day < date2.Day);

        public static bool operator >( MALDate date1, MALDate date2 ) =>
            date1.Year > date2.Year ||
            (date1.Year == date2.Year && date1.Month > date2.Month) ||
            (date1.Year == date2.Year && date1.Month == date2.Month && date1.Day > date2.Day);

        public static bool operator <=( MALDate date1, MALDate date2 ) =>
            date1 == date2 || date1 < date2;

        public static bool operator >=( MALDate date1, MALDate date2 ) =>
            date1 == date2 || date1 > date2;
    }
}
