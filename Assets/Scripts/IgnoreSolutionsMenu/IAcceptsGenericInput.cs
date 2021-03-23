namespace IgnoreSolutions.DuelistsOfTheRoses
{
    public interface IAcceptsGenericInput
    {
        bool _InterceptCancel { get; set; }

        void TranslateInput(int type, int playerIndex);
        void OnInputHandoff();
    }
}
