/*
 *	Copyright (C) 2007-2013 ARGUS TV
 *	http://www.argus-tv.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using ArgusTV.DataContracts;
using ArgusTV.UI.Process;
using ArgusTV.ServiceAgents;

namespace ArgusTV.WinForms.UserControls
{
    public partial class UpcomingOrActiveProgramsControl : UserControl
    {
        public UpcomingOrActiveProgramsControl()
        {
            InitializeComponent();
            _programsDataGridView.MouseUp += new MouseEventHandler(_programsDataGridView_MouseUp);
            _iconColumn.ValuesAreIcons = true;
            _scheduleNameColumn.Visible = false;
            WinFormsUtility.ResizeDataGridViewColumnsForCurrentDpi(_programsDataGridView);
        }

        public event MouseEventHandler GridMouseUp;
        public event EventHandler GridDoubleClick;

        public bool Sortable
        {
            get
            {
                return (_programsDataGridView.Columns[1].SortMode == DataGridViewColumnSortMode.Automatic);
            }
            set
            {
                foreach (DataGridViewColumn column in _programsDataGridView.Columns)
                {
                    if (column.Index > 0)
                    {
                        column.SortMode = DataGridViewColumnSortMode.Automatic;
                    }
                }
            }
        }

        public bool ShowScheduleName
        {
            get { return _scheduleNameColumn.Visible; }
            set { _scheduleNameColumn.Visible = value; }
        }

        private UpcomingOrActiveProgramsList _unfilteredUpcomingRecordings;

        public UpcomingOrActiveProgramsList UnfilteredUpcomingRecordings
        {
            get { return _unfilteredUpcomingRecordings; }
            set { _unfilteredUpcomingRecordings = value; }
        }

        public UpcomingOrActiveProgramsList UpcomingPrograms
        {
            get
            {
                return _upcomingProgramsListBindingSource.DataSource as UpcomingOrActiveProgramsList;
            }
            set
            {
                _upcomingProgramsListBindingSource.DataSource = value;
                _upcomingProgramsListBindingSource.ResetBindings(false);
            }
        }

        private ScheduleType _scheduleType = ScheduleType.Recording;

        public ScheduleType ScheduleType
        {
            get { return _scheduleType; }
            set { _scheduleType = value; }
        }

        public DataGridViewRowCollection Rows
        {
            get { return _programsDataGridView.Rows; }
        }

        public DataGridViewSelectedRowCollection SelectedRows
        {
            get { return _programsDataGridView.SelectedRows; }
        }

        public DataGridView.HitTestInfo HitTest(int x, int y)
        {
            return _programsDataGridView.HitTest(x, y);
        }

        public void RemoveUpcomingProgramsForSchedule(Guid scheduleId)
        {
            UpcomingOrActiveProgramsList upcomingProgramsList = _upcomingProgramsListBindingSource.DataSource as UpcomingOrActiveProgramsList;
            List<UpcomingOrActiveProgramView> toRemove = new List<UpcomingOrActiveProgramView>();
            foreach (UpcomingOrActiveProgramView upcomingView in upcomingProgramsList)
            {
                UpcomingProgram upcomingProgram = upcomingView.UpcomingProgram;
                if (upcomingProgram.ScheduleId == scheduleId)
                {
                    toRemove.Add(upcomingView);
                }
            }
            foreach (UpcomingOrActiveProgramView upcomingView in toRemove)
            {
                upcomingProgramsList.Remove(upcomingView);
            }
            _upcomingProgramsListBindingSource.ResetBindings(false);
        }

        public void RemoveUpcomingProgram(Guid scheduleId, Guid? guideProgramId, Guid channelId, DateTime startTime)
        {
            UpcomingOrActiveProgramsList upcomingProgramsList = _upcomingProgramsListBindingSource.DataSource as UpcomingOrActiveProgramsList;
            foreach (UpcomingOrActiveProgramView upcomingView in upcomingProgramsList)
            {
                UpcomingProgram upcomingProgram = upcomingView.UpcomingProgram;
                if (upcomingProgram.ScheduleId == scheduleId
                    && upcomingProgram.Channel.ChannelId == channelId
                    && upcomingProgram.GuideProgramId == guideProgramId
                    && upcomingProgram.StartTime == startTime)
                {
                    upcomingProgramsList.Remove(upcomingView);
                    break;
                }
            }
            _upcomingProgramsListBindingSource.ResetBindings(false);
        }

        void _programsDataGridView_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.GridMouseUp != null)
            {
                this.GridMouseUp(this, e);
            }
        }

        private void _programsDataGridView_DoubleClick(object sender, EventArgs e)
        {
            if (this.GridDoubleClick != null)
            {
                this.GridDoubleClick(this, e);
            }
        }

        private void _programsDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex != 0
                && e.RowIndex >= 0
                && e.RowIndex < _upcomingProgramsListBindingSource.Count)
            {
                UpcomingOrActiveProgramView programView = _programsDataGridView.Rows[e.RowIndex].DataBoundItem as UpcomingOrActiveProgramView;
                if (programView != null
                    && (programView.UpcomingProgram.IsCancelled
                        || (programView.UpcomingRecording != null && programView.UpcomingRecording.CardChannelAllocation == null)))
                {
                    e.CellStyle.ForeColor = Color.DarkGray;
                    e.CellStyle.SelectionForeColor = Color.Gray;
                }
                else
                {
                    e.CellStyle.ForeColor = Color.Black;
                    e.CellStyle.SelectionForeColor = Color.Black;
                }
                if (_programsDataGridView.Columns[e.ColumnIndex] is DataGridViewLinkColumn)
                {
                    if (_programsDataGridView.Rows[e.RowIndex].Selected)
                    {
                        ((DataGridViewLinkCell)_programsDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex]).LinkColor = e.CellStyle.SelectionForeColor;
                    }
                    else
                    {
                        ((DataGridViewLinkCell)_programsDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex]).LinkColor = e.CellStyle.ForeColor;
                    }
                }
            }
        }

        private void _programsDataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in _programsDataGridView.Rows)
            {
                Icon icon = Properties.Resources.TransparentIcon;
                string toolTip = null;
                UpcomingOrActiveProgramView programView = row.DataBoundItem as UpcomingOrActiveProgramView;
                if (programView != null)
                {
                    ProgramIconUtility.GetIconAndToolTip(_scheduleType, programView.CancellationReason, programView.IsPartOfSeries,
                        this.UnfilteredUpcomingRecordings ?? this.UpcomingPrograms, programView.UpcomingRecording, out icon, out toolTip);
                    if (programView.UpcomingRecording != null
                        && programView.CancellationReason == UpcomingCancellationReason.None)
                    {
                        row.Cells[2].ToolTipText = ProcessUtility.BuildRecordingInfoToolTip(programView.UpcomingRecording, "on");
                    }
                    else if (programView.ActiveRecording != null
                        && programView.CancellationReason == UpcomingCancellationReason.None)
                    {
                        row.Cells[2].ToolTipText = ProcessUtility.BuildRecordingInfoToolTip(programView.ActiveRecording, "on");
                    }
                    else
                    {
                        row.Cells[2].ToolTipText = null;
                    }
                }
                row.Cells[0].Value = icon;
                row.Cells[0].ToolTipText = toolTip;
            }
        }

        private void _programsDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3
                && e.RowIndex >= 0
                && e.RowIndex < _programsDataGridView.Rows.Count)
            {
                UpcomingOrActiveProgramView programView = _programsDataGridView.Rows[e.RowIndex].DataBoundItem as UpcomingOrActiveProgramView;
                if (programView.UpcomingProgram.GuideProgramId.HasValue)
                {
                    using (GuideServiceAgent tvGuideAgent = new GuideServiceAgent())
                    {
                        GuideProgram guideProgram = tvGuideAgent.GetProgramById(programView.UpcomingProgram.GuideProgramId.Value);
                        using (ProgramDetailsPopup popup = new ProgramDetailsPopup())
                        {
                            popup.Channel = programView.UpcomingProgram.Channel;
                            popup.GuideProgram = guideProgram;
                            Point location = Cursor.Position;
                            location.Offset(-250, -40);
                            popup.Location = location;
                            popup.ShowDialog(this);
                        }
                    }
                }
            }
        }
    }
}
