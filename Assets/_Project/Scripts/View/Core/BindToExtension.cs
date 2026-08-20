using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utilities.Lifetimes;
using Utilities.Reactive;
using Object = UnityEngine.Object;

namespace View.Core
{
    public static class BindToExtension
    {
        public static void BindTo(this GameObject go, Lifetime lifetime)
        {
            if (go == null)
            {
                Debug.LogError("[BindToExtension] GameObject is null, binding skipped");
                return;
            }
            
            lifetime.Bracket(
                opening: () => go.SetActive(true),
                closing: () => go.SetActive(false)
            );
        }

        public static void BindTo(this TMP_InputField input, Lifetime lifetime, IViewableProperty<string> property)
        {
            if (input == null)
            {
                Debug.LogError("[BindToExtension] TMP_InputField is null, binding skipped");
                return;
            }
            
            lifetime.Bracket(
                opening: () => input.onValueChanged.AddListener(ProxyValueToProperty),
                closing: () => input.onValueChanged.RemoveListener(ProxyValueToProperty)
            );

            void ProxyValueToProperty(string value)
            {
                property.Value = value;
            }

            property.Advise(lifetime, value => input.text = value);
        }

        public static void BindTo(this TMP_InputField input, Lifetime lifetime, Action<string> handler)
        {
            if (input == null)
            {
                Debug.LogError("[BindToExtension] TMP_InputField is null, binding skipped");
                return;
            }
            
            lifetime.Bracket(
                opening: () => input.onValueChanged.AddListener(ProxyFunc),
                closing: () => input.onValueChanged.RemoveListener(ProxyFunc)
            );

            void ProxyFunc(string value)
            {
                handler?.Invoke(value);
            }
        }

        public static void BindTo(this Button button, Lifetime lifetime, ISignal<Unit> signal)
        {
            if (button == null)
            {
                Debug.LogError("[BindToExtension] Button is null, binding skipped");
                return;
            }
            
            lifetime.Bracket(
                opening: () => button.onClick.AddListener(Fire),
                closing: () => button.onClick.RemoveListener(Fire)
            );

            void Fire()
            {
                signal.Fire();
            }
        }

        public static void BindTo(this Button button, Lifetime lifetime, Command command,
            Action<bool> interactionHandler = null,
            Action<bool> activeHandler = null)
        {
            if (button == null)
            {
                Debug.LogError("[BindToExtension] Button is null, binding skipped");
                return;
            }
            
            Lifetime commandLifetime = null;

            void RefreshExecutableState()
            {
                try
                {
                    commandLifetime?.Terminate();
                    commandLifetime = null;

                    var canExecute = command.IsEnabled.Value && command.IsInteractable.Value;
                    button.interactable = canExecute;
                    if (canExecute)
                    {
                        commandLifetime = lifetime.CreateNested().Lifetime;
                        button.BindTo(commandLifetime, command.ExecuteCommand);
                    }

                    interactionHandler?.Invoke(canExecute);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            command.IsInteractable.Advise(lifetime, _ => RefreshExecutableState());

            command.IsEnabled.Advise(lifetime, isEnabled =>
            {
                try
                {
                    button.gameObject.SetActive(isEnabled);
                    activeHandler?.Invoke(isEnabled);
                    RefreshExecutableState();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            });

            lifetime.OnTermination(() => commandLifetime?.Terminate());
        }

        public static void BindTo(this Button button, Lifetime lifetime, ISignal<int> signal, int index)
        {
            if (button == null)
            {
                Debug.LogError("[BindToExtension] Button is null, binding skipped");
                return;
            }
            
            lifetime.Bracket(
                opening: () => button.onClick.AddListener(Fire),
                closing: () => button.onClick.RemoveListener(Fire)
            );

            void Fire()
            {
                try
                {
                    signal.Fire(index);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public static void BindTo(this Button button, Lifetime lifetime, Action handler)
        {
            if (button == null)
            {
                Debug.LogError("[BindToExtension] Button is null, binding skipped");
                return;
            }
            
            lifetime.Bracket(
                opening: () => button.onClick.AddListener(handler.Invoke),
                closing: () => button.onClick.RemoveListener(handler.Invoke)
            );
        }

        public static void BindTo(this Toggle toggle, Lifetime lifetime, Action<bool> handler)
        {
            if (toggle == null)
            {
                Debug.LogError("[BindToExtension] Toggle is null, binding skipped");
                return;
            }
            
            lifetime.Bracket(
                opening: () => toggle.onValueChanged.AddListener(handler.Invoke),
                closing: () => toggle.onValueChanged.RemoveListener(handler.Invoke)
            );
        }

        public static void BindTo(this Slider slider, Lifetime lifetime, Action<float> handler)
        {
            if (slider == null)
            {
                Debug.LogError("[BindToExtension] Slider is null, binding skipped");
                return;
            }
            
            lifetime.Bracket(
                opening: () => slider.onValueChanged.AddListener(handler.Invoke),
                closing: () => slider.onValueChanged.RemoveListener(handler.Invoke)
            );
        }

        public static void BindTo(this Slider slider, Lifetime lifetime, IViewableProperty<float> property)
        {
            if (slider == null)
            {
                Debug.LogError("[BindToExtension] Slider is null, binding skipped");
                return;
            }
            
            lifetime.Bracket(
                opening: () => slider.onValueChanged.AddListener(OnSliderValueChanged),
                closing: () => slider.onValueChanged.RemoveListener(OnSliderValueChanged)
            );

            void OnSliderValueChanged(float value)
            {
                property.Value = value;
            }

            property.Advise(lifetime, value => slider.value = value);
        }

        public static void BindTo(this Toggle toggle, Lifetime lifetime, IViewableProperty<bool> property)
        {
            if (toggle == null)
            {
                Debug.LogError("[BindToExtension] Toggle is null, binding skipped");
                return;
            }
            
            lifetime.Bracket(
                opening: () => toggle.onValueChanged.AddListener(OnToggleValueChanged),
                closing: () => toggle.onValueChanged.RemoveListener(OnToggleValueChanged)
            );

            void OnToggleValueChanged(bool value)
            {
                property.Value = value;
            }

            property.Advise(lifetime, value => { toggle.isOn = value; });
        }

        public static void BindTo(this TMP_Text label, Lifetime lifetime, IReadonlyProperty<string> property)
        {
            if (label == null)
            {
                Debug.LogError("[BindToExtension] TMP_Text is null, binding skipped");
                return;
            }
            
            property.Advise(lifetime, value => label.text = value);
        }

        public static void BindTo<T>(this TMP_Text label, Lifetime lifetime, IReadonlyProperty<T> property)
        {
            if (label == null)
            {
                Debug.LogError("[BindToExtension] TMP_Text is null, binding skipped");
                return;
            }
            
            property.Advise(lifetime, value => label.SetText(value?.ToString()));
        }

        public static void BindTo<T>(this TMP_Text label, Lifetime lifetime, IReadonlyProperty<T> property,
            string format)
        {
            if (label == null)
            {
                Debug.LogError("[BindToExtension] TMP_Text is null, binding skipped");
                return;
            }
            
            property.Advise(lifetime, value => label.SetText(string.Format(format, value)));
        }

        public static void BindTo<T>(this TMP_Text label, Lifetime lifetime, IReadonlyProperty<T> first,
            IReadonlyProperty<T> second,
            string format)
        {
            if (label == null)
            {
                Debug.LogError("[BindToExtension] TMP_Text is null, binding skipped");
                return;
            }
            
            first.Compose(lifetime, second, (x, y) => label.SetText(string.Format(format, x, y)));
        }

        public static void BindTo(this Image filler, Lifetime lifetime, IReadonlyProperty<float> progressGetter)
        {
            if (filler == null)
            {
                Debug.LogError("[BindToExtension] Image is null, binding skipped");
                return;
            }
            
            progressGetter.Advise(lifetime, progress => filler.fillAmount = progress);
        }

        public static void BindTo(this IViewableProperty<GameObject> target, Lifetime lifetime,
            GameObject reference)
        {
            lifetime.Bracket(
                () =>
                {
                    try
                    {
                        target.Value = Object.Instantiate(reference);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        throw;
                    }
                },
                () =>
                {
                    try
                    {
                        target.Value = null;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            );
        }

        public static void BindTo(this Image image, Lifetime lifetime, Sprite sprite)
        {
            if (image == null)
            {
                Debug.LogError("[BindToExtension] Image is null, binding skipped");
                return;
            }
            
            var colorLoading = new Color(0.5f, 0.5f, 0.5f, 0.1f);
            lifetime.Bracket(() =>
                {
                    try
                    {
                        image.color = colorLoading;
                        image.sprite = null;
                        image.color = Color.white;
                        image.sprite = sprite;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        throw;
                    }
                },
                () =>
                {
                    try
                    {
                        image.color = colorLoading;
                        image.sprite = null;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            );
        }

        public static void BindTo(this TMP_Dropdown dropdown, Lifetime lifetime, List<string> variants,
            IViewableProperty<int> selected)
        {
            if (dropdown == null)
            {
                Debug.LogError("[BindToExtension] TMP_Dropdown is null, binding skipped");
                return;
            }
            
            dropdown.ClearOptions();
            dropdown.AddOptions(variants);
            dropdown.value = selected.Maybe.ValueOrDefault;

            var onValueChanged = new UnityAction<int>(OnValueChanged);

            dropdown.onValueChanged.AddListener(onValueChanged);
            lifetime.OnTermination(() => dropdown.onValueChanged.RemoveListener(onValueChanged));

            dropdown.RefreshShownValue();

            void OnValueChanged(int value)
            {
                try
                {
                    selected.Value = value;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public static void InteractableWhileTrue(this Toggle toggle, Lifetime lifetime,
            IReadonlyProperty<bool> property)
        {
            if (toggle == null)
            {
                Debug.LogError("[BindToExtension] Toggle is null, binding skipped");
                return;
            }
            
            property.Advise(lifetime, value => toggle.interactable = value);
        }
    }
}
