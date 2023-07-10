using UnityEngine;
using UnityEngine.UI;

namespace DoDo.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button optionButton;
        [SerializeField] private Button creditButton;
        [SerializeField] private Button quitButton;

        /*******************************************/
        /*              Unity Methods              */
        /*******************************************/

        /*******************************************/
        /*             Private Methods             */
        /*******************************************/

        /******************************************/
        /*             Public Methods             */
        /******************************************/
        public void PlayButton()
        {
            Loader.Load(Loader.Scene.LobbySelectionScene);
        }
        public void OptionButton()
        {
            // TODO
        }
        public void CreditButton()
        {
            // TODO
        }
        public void QuitButton()
        {
            Application.Quit();
        }

    }
}