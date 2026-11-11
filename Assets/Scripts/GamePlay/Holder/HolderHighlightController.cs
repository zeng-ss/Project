namespace CasualGame.CoffeeAway
{
    public class HolderHighlightController : TMonoSingleton<HolderHighlightController>
    {
        public bool IsActive => gameObject.activeSelf;

        protected override void Awake()
        {
            base.Awake();
            gameObject.SetActive(false);
        }

        public void SetHighlight(bool on)
        {
            gameObject.SetActive(on);
        }
    }
}