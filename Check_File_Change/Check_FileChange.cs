/*
 *	File:
 *		Check_File_Change.cs
 *
 *	Description:
 *		Check_File_Change is a simple program to check if a file has been modified in a specified timerange.
 *		It returns exit codes specified for Icinga2.
 *		This program uses the NDesk.Options library (http://www.ndesk.org/Options).
 *
 *	Author:
 *		Black-Dragon131 (bd131@black-dragon131.de)
 *
 *
 *	Copyright (C) 2016 Black-Dragon131 (www.black-dragon131.de)
 *
 *	This program is free software; you can redistribute it and/or
 *	modify it under the terms of the GNU General Public License
 *	as published by the Free Software Foundation; either version 2
 *	of the License, or (at your option) any later version.
 *
 *	This program is distributed in the hope that it will be useful,
 *	but WITHOUT ANY WARRANTY; without even the implied warranty of
 *	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *	GNU General Public License for more details.
 *
 *	You should have received a copy of the GNU General Public License
 *	along with this program; if not, write to the Free Software Foundation
 *	Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
 * 
*/

using System;
using System.IO;
using NDesk.Options;

namespace Check_File_Change
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			const string myVersion = "1.0";

			// init our variables
			int returnCode = 3;
			int maxMinutes = 5;

			string fullPath = null;

			bool showDebug = false;
			bool showHelp = false;
			bool include = false;
			bool warning = false;

			// setup our prameters
			var optionSet = new OptionSet () {
				{ "f=|file=", "file to check", f => fullPath = f },
				{ "m:|minutes:", "time in minutes (default: 5)", (int m) => maxMinutes = m },
				{ "i|include", "include modified time in output", i => include = i != null },
				{ "w|warning", "return warning instead of critical", w => warning = w != null },
				{ "v|verbose", "show debug messages", d => showDebug = d != null },
				{ "?|h|help",  "show this message and exit", h => showHelp = h != null }
			};

			try {
				optionSet.Parse (args);
			}
			catch (OptionException e) {
				Console.WriteLine (e.Message);
				Console.WriteLine ("Try `Check_File_Change --help' for more information.");
				return;
			}

			// we need a file to check!
			if (showHelp || fullPath == null) {
				ShowHelp (optionSet);
				return;
			}

			// does the file exist?
			if (File.Exists (fullPath)) {

				string fileName = Path.GetFileName(fullPath);
				// get the last modification date
				DateTime lastModified = File.GetLastWriteTime (fullPath);

				// we all love debug messages :)
				if (showDebug) {
					Console.WriteLine ();
					Console.WriteLine ("DEBUG - BEGIN");
					Console.WriteLine ("version: " + myVersion);
					Console.WriteLine ("file to check: " + fullPath);
					Console.WriteLine ("minutes: " + maxMinutes.ToString());
					Console.WriteLine ("last modified: " + lastModified.ToString ("dd.MM.yyyy HH:mm:ss"));
					Console.WriteLine ("DEBUG - END");
					Console.WriteLine ();
				}

				// let's create our time span...
				DateTime dt = DateTime.Now;
				TimeSpan span = dt.Subtract (lastModified);

				// ...and check if we are late
				if ((int)span.TotalMinutes > maxMinutes) {
					Console.WriteLine ( ((warning) ? "WARNING": "CRITICAL") + " - " + fileName + " didn't changed for " + span.Hours + " hours " + span.Minutes + " minutes!" );
					returnCode = (warning) ? 1: 2;
				} else {
					Console.WriteLine ( fileName + " is OK" + ((include) ? " - "+ lastModified.ToString ("dd.MM.yyyy HH:mm:ss"): "") );
					returnCode = 0;
				}
			} else {
				Console.Write (fullPath + " not found!");
			}

			// return code for icinga
			Environment.Exit( returnCode );
		}

		// give the user some help
		static void ShowHelp(OptionSet optionSet)
		{
			Console.WriteLine ("Description: Check_File_Change is a simple program to check if a file has been modified in a specified timerange.");
			Console.WriteLine ();
			Console.WriteLine ("Usage: Check_File_Change -f=\"PATH\\TO\\FILENAME\" [OPTIONS]");
			Console.WriteLine ();
			Console.WriteLine ("Options:");
			optionSet.WriteOptionDescriptions (Console.Out);
		}
	}
}
