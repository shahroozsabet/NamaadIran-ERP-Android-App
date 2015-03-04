/*
 * Copyright (C) 2009 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace NamaadDB.NotePad
{
	using Android.Content;
	using Android.Database;
	using Android.Database.Sqlite;
	using Android.Util;
	using System;

	public class NamaadDbAdapter_note
	{

		public const string KeyName = "name";
		public const string KeyDetail = "detail";
		public const string KeyRowId = "_id";

		// Database creation sql statement
		private static string DatabaseCreate;
		private static string TableDrop;

		private const string Tag = "NamaadDbAdapter";//For Debugging purpose

		private const string DirName = "NammadDB";
		private readonly string DBName = "NamaadDB";
		private static string dbPath;
		private static int DatabaseVersion = 1;
		private string DatabaseTable = "Cust";

		private DatabaseHelper dbHelper;
		private SQLiteDatabase db;

		private readonly Context ctx;

		/// <summary>
		/// Initializes a new instance of the <see cref="NamaadDbAdapter_note"/> class.
		/// Takes the context to allow the database to be opened/created
		/// </summary>
		/// <param name="ctx">the Context within which to work</param>
		public NamaadDbAdapter_note(Context ctx, string DBName, int DatabaseVersionArg, string DatabaseCreateArg, string TableDropArg)
		{
			this.ctx = ctx;
			dbPathPrep();
			this.DBName = DBName;
			DatabaseVersion = DatabaseVersionArg;
			DatabaseCreate = DatabaseCreateArg;
			TableDrop = TableDropArg;
		}

		/// <summary>
		/// Open the Namaad database. If it cannot be opened, try to create a new
		/// instance of the database. If it cannot be created, throw an exception to
		/// signal the failure
		/// </summary>
		/// <returns>this (self reference, allowing this to be chained in an initialization call)</returns>
		/// <exception cref="SQLException">if the database could be neither opened or created</exception>
		public NamaadDbAdapter_note Open()
		{
			dbHelper = new DatabaseHelper(ctx);
			db = dbHelper.WritableDatabase;
			return this;
		}
		private void dbPathPrep()
		{
			//path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
			//filename = Path.Combine(path, Filename);

			Java.IO.File pathToExternalStorage = Android.OS.Environment.ExternalStorageDirectory;
			Java.IO.File qualifiedDir = new Java.IO.File(pathToExternalStorage + "/" + DirName + "/");
			dbPath = qualifiedDir.Path + "/" + DBName;
			if (qualifiedDir.Mkdirs())
			{
				Console.WriteLine("Directory created.");
			}
			else
			{
				Console.WriteLine("Directory was created or \n failed in it creation!");
			}
		}

		public void Close()
		{
			dbHelper.Close();
		}

		/// <summary>
		/// Create a new Cust using the name and detail provided. If the Cust is
		/// successfully created return the new rowId for that Cust, otherwise return
		/// a -1 to indicate failure.
		/// </summary>
		/// <param name="name">the name of the Cust</param>
		/// <param name="detail">the detail of the Cust</param>
		/// <returns>rowId or -1 if failed</returns>
		public long CreateCust(string name, string detail)
		{
			var initialValues = new ContentValues();
			initialValues.Put(KeyName, name);
			initialValues.Put(KeyDetail, detail);

			return db.Insert(DatabaseTable, null, initialValues);
		}

		/// <summary>
		/// Delete the Cust with the given rowId
		/// </summary>
		/// <param name="rowId">id of Cust to delete</param>
		/// <returns>true if deleted, false otherwise</returns>
		public bool DeleteRec(long rowId)
		{
			return db.Delete(DatabaseTable, KeyRowId + "=" + rowId, null) > 0;
		}

		/// <summary>
		/// Return a Cursor over the list of all Cust in the database
		/// </summary>
		/// <returns>A Cursor over all Cust</returns>
		public ICursor FetchAllCusts()
		{
			ICursor cursor = null;
			cursor = this.db.Query(DatabaseTable, new[] { KeyRowId, KeyName, KeyDetail }, null, null, null, null, null);

			return cursor;
		}

		/// <summary>
		/// Return a Cursor positioned at the Cust that matches the given rowId
		/// </summary>
		/// <param name="rowId">id of Cust to retrieve</param>
		/// <returns>A cursor positioned to matching Cust, if found</returns>
		/// <exception cref="SQLException">if Cust could not be found/retrieved</exception>
		public ICursor FetchCust(long rowId)
		{
			ICursor cursor = null;
			cursor = this.db.Query(
			   true,
			   DatabaseTable,
			   new[] { KeyRowId, KeyName, KeyDetail },
			   KeyRowId + "=" + rowId,
			   null,
			   null,
			   null,
			   null,
			   null);

			if (cursor != null)
			{
				cursor.MoveToFirst();
			}
			return cursor;
		}

		/// <summary>
		/// Update the Cust using the details provided. The Cust to be updated is
		/// specified using the rowId, and it is altered to use the title and body
		/// values passed in
		/// </summary>
		/// <param name="rowId">id of note to update</param>
		/// <param name="name">value to set note title to</param>
		/// <param name="detail">value to set note body to</param>
		/// <returns>true if the note was successfully updated, false otherwise</returns>
		public bool UpdateCust(long rowId, string name, string detail)
		{
			var args = new ContentValues();
			args.Put(KeyName, name);
			args.Put(KeyDetail, detail);

			return db.Update(DatabaseTable, args, KeyRowId + "=" + rowId, null) > 0;
		}

		private class DatabaseHelper : SQLiteOpenHelper
		{
			internal DatabaseHelper(Context context)
				: base(context, dbPath, null, DatabaseVersion)
			{
			}

			public override void OnCreate(SQLiteDatabase db)
			{
				db.ExecSQL(DatabaseCreate);
			}

			public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
			{
				Log.Wtf(Tag, "Upgrading database from version " + oldVersion + " to " + newVersion + ", which will destroy all old data");
				db.ExecSQL(TableDrop);
				this.OnCreate(db);
			}
		}


	}
}