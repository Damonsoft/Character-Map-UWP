﻿using CharacterMap.Controls;
using CharacterMap.Core;
using CharacterMap.Helpers;
using CharacterMap.Models;
using GalaSoft.MvvmLight;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CharacterMap.ViewModels
{
    public enum PrintLayout
    {
        Grid,
        List,
        TwoColumn
    }

    public enum GlyphAnnotation
    { 
        None = 0,
        UnicodeHex = 1,
        UnicodeIndex = 2
    }

    public class UnicodeCategoryModel : ViewModelBase
    {
        public UnicodeGeneralCategory Category { get; }

        private bool _isSelected = true;
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        public string DisplayName { get; }

        public UnicodeCategoryModel(UnicodeGeneralCategory category)
        {
            Category = category;
            DisplayName = Humanizer.EnumHumanizeExtensions.Humanize(category);
        }
    }

    public class PrintViewModel : ViewModelBase
    {
        public FontVariant Font { get; set; }

        public TypographyFeatureInfo Typography { get; set; }

        public FontFamily FontFamily { get; set; }

        private bool _showMargins = false;
        public bool ShowMargins
        {
            get => _showMargins;
            set => Set(ref _showMargins, value);
        }

        private bool _showBorders = false;
        public bool ShowBorders
        {
            get => _showBorders;
            set => Set(ref _showBorders, value);
        }

        private bool _showWhitespace = true;
        public bool ShowWhitespace
        {
            get => _showWhitespace;
            set
            {
                if (value != _showWhitespace)
                {
                    _showWhitespace = value;
                    UpdateCharacters();
                    RaisePropertyChanged();
                }
            }
        }

        private bool _showColorGlyphs = true;
        public bool ShowColorGlyphs
        {
            get => _showColorGlyphs;
            set => Set(ref _showColorGlyphs, value);
        }

        private double _glyphSize = 70d;
        public double GlyphSize
        {
            get => _glyphSize;
            set => Set(ref _glyphSize, value);
        }

        private double _horizontalMargin = 44d;
        public double HorizontalMargin
        {
            get => _horizontalMargin;
            set => Set(ref _horizontalMargin, value);
        }

        private double _verticalMargin = 44d;
        public double VerticalMargin
        {
            get => _verticalMargin;
            set => Set(ref _verticalMargin, value);
        }

        private GlyphAnnotation _annotation = GlyphAnnotation.None;
        public GlyphAnnotation Annotation
        {
            get => _annotation;
            set => Set(ref _annotation, value);
        }

        private PrintLayout _layout = PrintLayout.Grid;
        public PrintLayout Layout
        {
            get => _layout;
            set => Set(ref _layout, value);
        }

        private Orientation _orientation = Orientation.Vertical;
        public Orientation Orientation
        {
            get => _orientation;
            set => Set(ref _orientation, value);
        }
        public IReadOnlyList<Character> Characters { get; set; }

        private List<UnicodeCategoryModel> _categories;
        public List<UnicodeCategoryModel> Categories
        {
            get => _categories;
            private set => Set(ref _categories, value);
        }


        public bool IsPortrait => Orientation == Orientation.Vertical;

        internal CharacterGridViewTemplateSettings GetTemplateSettings()
        {
            return new CharacterGridViewTemplateSettings
            {
                Size = GlyphSize,
                ShowColorGlyphs = ShowColorGlyphs,
                Annotation = GlyphAnnotation.None,
                Typography = Typography,
                FontFamily = FontFamily,
                FontFace = Font.FontFace
            };
        }

        public void UpdateCategories(List<UnicodeCategoryModel> value)
        {
            _categories = value;
            UpdateCharacters();
            RaisePropertyChanged(nameof(Categories));
        }

        private void UpdateCharacters()
        {
            // Fast path : all characters;
            if (!Categories.Any(c => !c.IsSelected) && ShowWhitespace)
            {
                Characters = Font.Characters;
                return;
            }

            // Filter characters

            var chars = Font.Characters.AsEnumerable();
            if (!ShowWhitespace)
                chars = Font.Characters.Where(c => !Unicode.IsWhiteSpaceOrControl(c.UnicodeIndex));

            foreach (var cat in Categories.Where(c => !c.IsSelected))
                chars = chars.Where(c => !Unicode.IsInCategory(c.UnicodeIndex, cat.Category));

            Characters = chars.ToList();
        }

        private PrintViewModel()
        {
            Categories = CreateCategoriesList(null);
        }

        public static List<UnicodeCategoryModel> CreateCategoriesList(PrintViewModel viewModel)
        {
            var list = Enum.GetValues(typeof(UnicodeGeneralCategory)).OfType<UnicodeGeneralCategory>().Select(e => new UnicodeCategoryModel(e)).ToList();
            
            if (viewModel != null)
                for (int i = 0; i < list.Count; i++)
                    list[i].IsSelected = viewModel.Categories[i].IsSelected;

            return list;
        }

        public static PrintViewModel Create(FontMapViewModel viewModel)
        {
            return new PrintViewModel
            {
                ShowColorGlyphs = viewModel.ShowColorGlyphs,
                Typography = viewModel.SelectedTypography,
                FontFamily = viewModel.FontFamily,
                Font = viewModel.SelectedVariant,
                Characters = viewModel.SelectedVariant.Characters,
                Annotation = viewModel.Settings.GlyphAnnotation
            };
        }
    }
}
