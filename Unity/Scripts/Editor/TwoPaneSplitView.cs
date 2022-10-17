using UnityEngine.UIElements;

namespace SK.Libretro.Unity.Editor
{
    internal sealed class TwoPaneSplitView : UnityEngine.UIElements.TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<TwoPaneSplitView, UxmlTraits> { }

        public TwoPaneSplitView()
        : base()
        {
        }

        public TwoPaneSplitView(int fixedPaneIndex, float fixedPaneStartDimension, TwoPaneSplitViewOrientation orientation)
        : base(fixedPaneIndex, fixedPaneStartDimension, orientation)
        {
        }
    }
}
