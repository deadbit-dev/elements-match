using Scellecs.Morpeh;
using Scellecs.Morpeh.Providers;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class ButtonProvider : MonoProvider<ButtonComponent>
{
    protected override void Initialize()
    {
        base.Initialize();
        ref var buttonComponent = ref this.GetData();
        buttonComponent.name = this.gameObject.name;
        buttonComponent.isClicked = false;
    }

    public void OnClick()
    {
        ref var buttonComponent = ref this.GetData();
        buttonComponent.isClicked = true;
    }
}