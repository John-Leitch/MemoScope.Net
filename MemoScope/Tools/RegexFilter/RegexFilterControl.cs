using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MemoScope.Tools.RegexFilter
{
    public partial class RegexFilterControl : UserControl
    {
        private Regex regex;
        public event Action<Regex> RegexApplied;
        public event Action RegexCancelled;

        public RegexFilterControl() => InitializeComponent();

        private void tbRegex_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    Apply();
                    break;
                case Keys.Escape:
                    Cancel();
                    break;
            }
        }

        private void Cancel()
        {
            RegexCancelled?.Invoke();
            tbRegex.BackColor = Color.LightGray;
        }

        private void Apply()
        {
            try
            {
                regex = cbIgnoreCase.Checked ? new Regex(tbRegex.Text) : new Regex(tbRegex.Text, RegexOptions.IgnoreCase);

                RegexApplied?.Invoke(regex);
                tbRegex.BackColor = Color.LightGreen;
            }
            catch (ArgumentException)
            {
                Cancel();
                tbRegex.BackColor = Color.Orange;
            }
        }

        private void btnApply_Click(object sender, EventArgs e) => Apply();

        private void btnCancel_Click(object sender, EventArgs e) => Cancel();

        private void cbIgnoreCase_CheckedChanged(object sender, EventArgs e) => Apply();
    }
}
