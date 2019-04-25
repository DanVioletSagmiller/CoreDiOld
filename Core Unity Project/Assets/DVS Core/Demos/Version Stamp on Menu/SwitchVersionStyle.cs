namespace Dvs.Core
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public class SwitchVersionStyle : MonoBehaviour {

        public UiTextVersion UiTextVersion;

        public Text ButtonText;

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
        public string[] Format =
        {
            "%product% v%major%.%minor%.%build% @%completed%",
            "%major%.%minor%.%build%",
            "%user% %created%",
            "%product% v%major%.%minor%.%build% @%completed%\r\n%user%\r\n%log%",
        };

        public void Start()
        {
            NextStyle();
        }

        private int FormatIndex = 100000;

        public void NextStyle()
        {
            if (++FormatIndex > Format.Length - 1) FormatIndex = 0;
            UiTextVersion.DisplayFormat = Format[FormatIndex];
            UiTextVersion.SetVersionText();
            ButtonText.text = (FormatIndex + 1) + " / " + Format.Length;
             
        }
    }
}