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

namespace NamaadMobile.NotePad
{
	using Android.App;
	using Android.Content;
	using Android.Database;
	using Android.OS;
	using Android.Views;
	using Android.Widget;

	[Activity(Label = "NotePad")]
	public class NmdDB : ListActivity
	{
		private const int ActivityCreate = 0;
		private const int ActivityEdit = 1;

		private const int InsertId = Menu.First;
		private const int DeleteId = Menu.First + 1;

		private NamaadDbAdapter_note dbHelper;

		// Database creation sql statement
		private readonly static string DatabaseCreate = "create table Cust (_id integer primary key autoincrement, "
			+ "name text not null, detail text not null);";
		private readonly static string TableDrop = "DROP TABLE IF EXISTS Cust";
		private readonly static short DatabaseVersion = 1;
		private readonly static string DBName = "NamaadDB";


		// Called when the activity is first created.
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.recs_list);

			this.dbHelper = new NamaadDbAdapter_note(this, DBName, DatabaseVersion, DatabaseCreate, TableDrop);
			this.dbHelper.Open();
			this.FillData();

			RegisterForContextMenu(ListView);

		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			base.OnCreateOptionsMenu(menu);
			menu.Add(0, InsertId, 0, Resource.String.menu_insert);
			return true;
		}

		public override bool OnMenuItemSelected(int featureId, IMenuItem item)
		{
			switch (item.ItemId)
			{
				case InsertId:
					this.CreateCust();
					return true;
			}

			return base.OnMenuItemSelected(featureId, item);
		}

		public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
		{
			base.OnCreateContextMenu(menu, v, menuInfo);
			menu.Add(0, DeleteId, 0, Resource.String.menu_delete);
		}

		public override bool OnContextItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case DeleteId:
					var info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;
					this.dbHelper.DeleteRec(info.Id);
					this.FillData();
					return true;
			}
			return base.OnContextItemSelected(item);
		}

		protected override void OnListItemClick(ListView l, View v, int position, long id)
		{
			base.OnListItemClick(l, v, position, id);
			var i = new Intent(this, typeof(CustEdit));
			i.PutExtra(NamaadDbAdapter_note.KeyRowId, id);
			StartActivityForResult(i, ActivityEdit);
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			this.FillData();
		}

		private void FillData()
		{
			ICursor custCursor = this.dbHelper.FetchAllCusts();
			this.StartManagingCursor(custCursor);

			// Create an array to specify the fields we want to display in the list (only name)
			var from = new[] { NamaadDbAdapter_note.KeyName };

			// and an array of the fields we want to bind those fields to (in this case just recRowTV)
			var to = new[] { Resource.Id.recRowTV };

			// Now create a simple cursor adapter and set it to display
			var Custs =
				new SimpleCursorAdapter(this, Resource.Layout.recs_row, custCursor, from, to);
			this.ListAdapter = Custs;
		}

		private void CreateCust()
		{
			var i = new Intent(this, typeof(CustEdit));
			this.StartActivityForResult(i, ActivityCreate);
		}
	}
}