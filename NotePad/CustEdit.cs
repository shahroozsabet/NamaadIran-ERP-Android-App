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
	using Android.Database;
	using Android.OS;
	using Android.Widget;
	using Java.Lang;

	[Activity(Label = "CustEdit")]
	public class CustEdit : Activity
	{
		private EditText nameText;
		private EditText detailText;
		private Long rowId;
		private NamaadDbAdapter_note dbHelper;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			this.dbHelper = new NamaadDbAdapter_note(this, "NamaadDB", 1, null, null);
			this.dbHelper.Open();

			SetContentView(Resource.Layout.cust_edit);
			SetTitle(Resource.String.edit_rec);

			this.nameText = (EditText)FindViewById(Resource.Id.name);
			this.detailText = (EditText)FindViewById(Resource.Id.detail);

			var confirmButton = (Button)FindViewById(Resource.Id.confirm);

			this.rowId = ((savedInstanceState == null) ? null : savedInstanceState.GetSerializable(NamaadDbAdapter_note.KeyRowId)) as Long;

			if (this.rowId == null)
			{
				var extras = Intent.Extras;
				this.rowId = extras != null ? new Long(extras.GetLong(NamaadDbAdapter_note.KeyRowId))
										: null;
			}

			this.PopulateFields();
			confirmButton.Click += delegate
				{
					SetResult(Result.Ok);
					this.Finish();
				};
		}

		private void PopulateFields()
		{
			if (this.rowId == null)
			{
				return;
			}

			ICursor note = this.dbHelper.FetchCust(this.rowId.LongValue());
			this.StartManagingCursor(note);
			this.nameText.SetText(note.GetString(note.GetColumnIndexOrThrow(NamaadDbAdapter_note.KeyName)), TextView.BufferType.Normal);
			this.detailText.SetText(note.GetString(note.GetColumnIndexOrThrow(NamaadDbAdapter_note.KeyDetail)), TextView.BufferType.Normal);
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			this.SaveState();
			outState.PutSerializable(NamaadDbAdapter_note.KeyRowId, this.rowId);
		}


		protected override void OnPause()
		{
			base.OnPause();
			this.SaveState();
		}

		protected override void OnResume()
		{
			base.OnResume();
			this.PopulateFields();
		}

		private void SaveState()
		{
			string name = this.nameText.Text;
			string detail = this.detailText.Text;

			if (this.rowId == null)
			{
				long id = this.dbHelper.CreateCust(name, detail);
				if (id > 0)
				{
					this.rowId = new Long(id);
				}
			}
			else
			{
				this.dbHelper.UpdateCust(this.rowId.LongValue(), name, detail);
			}
		}

	}
}