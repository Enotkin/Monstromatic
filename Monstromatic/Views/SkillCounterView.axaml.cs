﻿using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Monstromatic.ViewModels;
using ReactiveUI;

namespace Monstromatic.Views;

public partial class SkillCounterView : ReactiveUserControl<SkillCounterViewModel>
{
    public SkillCounterView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}