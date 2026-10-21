using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace EventEase.Components;

/// <summary>
/// An <see cref="InputText"/> that updates its bound value on every keystroke
/// (oninput) instead of on blur, enabling live two-way binding previews while
/// keeping full EditForm validation support.
/// </summary>
public class LiveInputText : InputText
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "input");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "class", CssClass);
        builder.AddAttribute(3, "value", CurrentValueAsString);
        builder.AddAttribute(4, "oninput",
            EventCallback.Factory.CreateBinder<string?>(this, v => CurrentValueAsString = v, CurrentValueAsString));
        builder.CloseElement();
    }
}
