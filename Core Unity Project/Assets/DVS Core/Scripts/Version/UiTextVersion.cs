namespace Dvs.Core
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Text))]
    public class UiTextVersion : MonoBehaviour
    {
        [Tooltip(
@"%product% -> Product Name
%major% -> Major Revision #
%minor% -> Minor Revision #
%build% -> Build Revision #
%user% -> User Display Name
%created% -> Date Created
%completed% -> Date Completed
%url% -> Product URL
%log% -> Change Log")]

        public string DisplayFormat = "%product% v%major%.%minor%.%build%";

        public Product Product;

        void Start()
        {
            SetVersionText();
        }

        public void SetVersionText()
        { 
            var text = GetComponent<Text>();
            var completed = Product.CurrentVersion.Completed;
            if (string.IsNullOrEmpty(completed)) completed = "[in progress...]";

            //%product% -> Product Name
            //%major% -> Major Revision #
            //%minor% -> Minor Revision #
            //%build% -> Build Revision #
            //%user% -> User Display Name
            //%created% -> Date Created
            //%completed% -> Date Completed
            //%url% -> Product URL
            //%log% -> Change Log
            var v = Product.CurrentVersion;
            text.text = DisplayFormat
                .Replace("%product%", Product.name)
                .Replace("%major%", v.Major.ToString())
                .Replace("%minor%", v.Minor.ToString())
                .Replace("%build%", v.Build.ToString())
                .Replace("%user%", v.User)
                .Replace("%created%", v.Began)
                .Replace("%completed%", v.Completed)
                .Replace("%url%", Product.Url)
                .Replace("%log%", v.ChangeLog);
        }
    }
}