using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Perforce.P4;
using CenterImages = P4EXP.P4ObjectTreeListView.CenterImages;
using LeftImages = P4EXP.P4ObjectTreeListView.LeftImages;
using RightImages = P4EXP.P4ObjectTreeListView.RightImages;

namespace P4EXP
{
	public class P4FileTreeListViewItem : P4ObjectTreeListViewItem
	{
		private FileMetaData _fileData = null;
		public FileMetaData FileData
		{
			get { return _fileData; }
			set
			{
				_fileData = value;
				if (Fields != null)
				{
					InitSubitems(Fields);
				}
				SelectImagesFromMetaData();
			}
		}

		IList<object> Fields = null;

		//public string FileName { get; set; }
		//public string FileFolder { get; set; }

        public P4FileTreeListViewItem(TreeListViewItem parentItem, FileMetaData fileData, params object[] fields)
            : this(parentItem, fileData, fields.ToList<object>())
        {
        }

        public P4FileTreeListViewItem(TreeListViewItem parentItem, FileMetaData fileData, IList<object> fields)
            : base()
        {
            ParentItem = parentItem;
            Fields = fields;
            FileData = fileData;

            Tag = fileData;

            //FileName = fileData.LocalPath.GetFileName();
            //FileFolder = fileData.LocalPath.GetFileName();
        }

        public P4FileTreeListViewItem(TreeListViewItem parentItem, string path, FileMetaData fileData)
			:base()
		{
            _fileData = null; //fileData; 
			ParentItem = parentItem;
			Fields = null;
            Tag = fileData;

			ImageIndex = (int)CenterImages.Portrait;
            AddSubitem(path, 0);

            this.FullLine = true;
        }

		private void SelectImagesFromMetaData()
		{
            if (FileData == null || FileData.DepotPath==null)
            {
                return;
            }

			// first check if it is a directory
			if (FileData.DepotPath.Path.EndsWith("/..."))
			{
				ImageIndex = (int) CenterImages.FolderIcon;
			}
			else
			{
				// select the center images
				// Center image may be should be based on file type
				ImageIndex = (int) CenterImages.FileDepotIcon;
				if (FileData.Unresolved)
				{
					CenterImageIndices.Add((int)CenterImages.ResolveBadge);
				}
				//Left images
				if (FileData.Action == FileAction.Edit)
				{
					LeftImageIndices.Add((int)LeftImages.EditBadge);
				}
				else if (FileData.Action == FileAction.Add)
				{
					LeftImageIndices.Add((int)LeftImages.AddBadge);
				}
				else if (FileData.Action == FileAction.Delete)
				{
					LeftImageIndices.Add((int)LeftImages.DeleteBadge);
				}
				else if (FileData.Action == FileAction.Branch)
				{
					LeftImageIndices.Add((int)LeftImages.BranchBadge);
				}
				else if (FileData.Action == FileAction.MoveDelete)
				{
					LeftImageIndices.Add((int)LeftImages.DeleteMoveBadge);
				}
				else if (FileData.Action == FileAction.MoveAdd)
				{
					LeftImageIndices.Add((int)LeftImages.AddMoveBadge);
				}
				else if (FileData.Action == FileAction.Integrate)
				{
					LeftImageIndices.Add((int)LeftImages.IntegrateBadge);
				}
				if (FileData.OurLock)
				{
					LeftImageIndices.Add((int)LeftImages.LockBadge);
				}
				//Right images
				if (FileData.HeadRev > 0)
				{
					if (FileData.IsStale)
					{
						RightImageIndices.Add((int)RightImages.OutOfSyncBadge);
					}
					else
					{
						RightImageIndices.Add((int)RightImages.HeadBadge);
					}
				}
				if (FileData.OtherActions != null)
				{
					foreach (FileAction action in FileData.OtherActions)
					{
						if (action == FileAction.Edit)
						{
							RightImageIndices.Add((int)RightImages.EditOtherBadge);
						}
						else if (action == FileAction.Add)
						{
							RightImageIndices.Add((int)RightImages.AddOtherBadge);
						}
						else if (action == FileAction.Delete)
						{
							RightImageIndices.Add((int)RightImages.DeleteOtherBadge);
						}
						else if (action == FileAction.BranchFrom)
						{
							RightImageIndices.Add((int)RightImages.BranchOtherBadge);
						}
						else if (action == FileAction.MoveDelete)
						{
							RightImageIndices.Add((int)RightImages.DeleteMoveOtherBadge);
						}
						else if (action == FileAction.MoveAdd)
						{
							RightImageIndices.Add((int)RightImages.AddMoveOtherBadge);
						}
						else if (action == FileAction.Integrate)
						{
							RightImageIndices.Add((int)RightImages.IntegrateOtherBadge);
						}
					}
				}
			}

		}

		public enum SubItemFlag
		{
			None = 0,
			LocalFullPath,
			LocalFileName,
			LocalFolder,
			DepotFullPath,
			DepotFileName,
			DepotDepotFolder,
			Type,
			Mapped,
			Shelved,
			HaveRevision,
			HeadRevision,
			HaveSlashHeadRevision,
			HeadAction,
			HeadChange,
			HeadModTime,
			Digest,
			Size,
			Action,
			Resolved,
			Unresolved,
			ResolvedStatus,
			Reresolvable,
			OurLock,
			Change,
			FileExtension
		}
		private void InitSubitems(IList<object> fields)
		{
			this.SubItems.Clear();
			for (int idx = 0; idx < fields.Count; idx++)
			{
				object value = null;
				object field = fields[idx];
				if (field is SubItemFlag)
				{
					SubItemFlag flag = (SubItemFlag)field;
					switch (flag)
					{
						case SubItemFlag.None:
							value = string.Empty;
							break;
						case SubItemFlag.LocalFullPath:
							value = FileData.LocalPath.Path;
							break;
						case SubItemFlag.LocalFileName:
							value = FileData.LocalPath.GetFileName();
							break;
						case SubItemFlag.LocalFolder:
							value = FileData.LocalPath.GetDirectoryName();
							break;
						case SubItemFlag.DepotFullPath:
							value = FileData.DepotPath.Path;
							break;
						case SubItemFlag.DepotFileName:
							value = FileData.DepotPath.GetFileName();
							break;
						case SubItemFlag.DepotDepotFolder:
							value = FileData.DepotPath.GetDirectoryName();
							break;
						case SubItemFlag.Type:
							value = FileData.Type;
							break;
						case SubItemFlag.Mapped:
							value = FileData.IsMapped ? "*" : string.Empty;
							break;
						case SubItemFlag.Shelved:
							value = FileData.Shelved ? "*" : string.Empty;
							break;
						case SubItemFlag.HaveRevision:
							if (FileData.HaveRev >= 0)
							{
								value = string.Format("#{0}", FileData.HaveRev);
							}
							else
							{
								value = "#0";
							}
							break;
						case SubItemFlag.HeadRevision:
							if (FileData.HeadRev >= 0)
							{
								value = string.Format("#{0}", FileData.HeadRev);
							}
							else
							{
								value = "#0";
							}
							break;
						case SubItemFlag.HaveSlashHeadRevision:
							if (FileData.HaveRev >= 0)
							{
								value = string.Format("#{0}/#{1}", FileData.HaveRev, FileData.HeadRev);
							}
							else
							{
								value = string.Format("0/#{0}", FileData.HeadRev);
							}
							break;
						case SubItemFlag.HeadAction:
							value = FileData.HeadAction;
							break;
						case SubItemFlag.HeadChange:
							value = FileData.HeadChange;
							break;
						case SubItemFlag.HeadModTime:
							//if (Preferences.LocalSettings.GetBool("P4Date_format", true))
							//{
							//	value = FileData.HeadModTime.ToString("yyyy/MM/dd HH:mm:ss");
							//}
							//else
							//{
								value = FileData.HeadModTime.ToString();
							//}
							break;
						case SubItemFlag.Digest:
							value = FileData.Digest;
							break;
						case SubItemFlag.Size:
							if (FileData.FileSize >= 0)
							{
								value = PrettyPrintFileSize(FileData.FileSize);
							}
							else
							{
								value = string.Empty;
							}
							break;
						case SubItemFlag.Action:
							value = FileData.Action;
							break;
						case SubItemFlag.Resolved:
							value = FileData.Resolved ? "*" : string.Empty;
							break;
						case SubItemFlag.Unresolved:
							value = FileData.Unresolved ? "*" : string.Empty;
							break;
						case SubItemFlag.ResolvedStatus:
							value = string.Empty;
							//if (FileData.Resolved)
							//{
							//	value = Resources.P4FileTreeListViewItem_Resolved;
							//}
							//if (FileData.Unresolved)
							//{
							//	value = Resources.P4FileTreeListViewItem_Unresolved;
							//}
							break;
						case SubItemFlag.Reresolvable:
							value = FileData.Reresolvable ? "*" : string.Empty;
							break;
						case SubItemFlag.OurLock:
							value = FileData.OtherLock ? "*" : string.Empty;
							break;
						case SubItemFlag.FileExtension:
							value = System.IO.Path.GetExtension(FileData.DepotPath.GetFileName());
							break;
					}
				}
				else
				{
					value = field;
				}
				AddSubitem(value, idx);
			}
		}
	}
}
