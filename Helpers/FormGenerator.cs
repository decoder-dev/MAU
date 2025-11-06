using System;
using System.IO;

namespace NetworkMonitoringSystem.Helpers
{
    public static class FormGenerator
    {
        public static void GenerateForms()
        {
            var basePath = "Forms";
            var categories = new[]
            {
                ("Devices", 30),
                ("Requests", 30),
                ("Reports", 30),
                ("Settings", 30),
                ("Admin", 30)
            };

            int formNumber = 1;
            foreach (var (category, count) in categories)
            {
                var categoryPath = Path.Combine(basePath, category);
                if (!Directory.Exists(categoryPath))
                    Directory.CreateDirectory(categoryPath);

                for (int i = 1; i <= count; i++)
                {
                    var formName = $"Form{formNumber:000}";
                    var fileName = $"{formName}.cs";
                    var filePath = Path.Combine(categoryPath, fileName);

                    if (!File.Exists(filePath))
                    {
                        var content = $@"using System;
using System.Windows.Forms;

namespace NetworkMonitoringSystem.Forms.{category}
{{
    public partial class {formName} : Form
    {{
        public {formName}()
        {{
            InitializeComponent();
        }}

        private void InitializeComponent()
        {{
            this.Text = ""{category} - Form {formNumber}"";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
        }}
    }}
}}";
                        File.WriteAllText(filePath, content);
                    }
                    formNumber++;
                }
            }
        }
    }
}

