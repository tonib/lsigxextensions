using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames
{

	// TODO: Do not use timestamp, try to use version.Timestamp saved in kb datatabase has a limted precission, and comparissons may fail!!!
	/// <summary>
	/// Stores a kbobject timestamp. 
	/// </summary>
	public interface ITimestamped
	{
		/// <summary>
		/// Object timestamp
		/// </summary>
		DateTime Timestamp { get; set; }
	}

	static public class ITimestampedEx
	{
		static public bool SameSecond(this ITimestamped timestamped, DateTime timestamp)
		{
			// TODO: Do not use timestamp, try to use version.Timestamp saved in kb datatabase has a limted precission, and comparissons may fail!!!
			return SameSecond(timestamped.Timestamp, timestamp);
		}

		static public bool SameSecond(DateTime timpestamp1, DateTime timestamp2)
		{
			DateTime t1 = new DateTime(timpestamp1.Year, timpestamp1.Month, timpestamp1.Day, timpestamp1.Hour, timpestamp1.Minute, timpestamp1.Second),
				t2 = new DateTime(timestamp2.Year, timestamp2.Month, timestamp2.Day, timestamp2.Hour, timestamp2.Minute, timestamp2.Second);
			return t1 == t2;
		}
	}

}
