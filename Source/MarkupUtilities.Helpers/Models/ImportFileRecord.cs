using MarkupUtilities.Helpers.Exceptions;

namespace MarkupUtilities.Helpers.Models
{
  public class ImportFileRecord
  {
    public string DocumentIdentifier { get; set; }
    public int FileOrder { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int MarkupType { get; set; }
    public int? FillA { get; set; }
    public int? FillR { get; set; }
    public int? FillG { get; set; }
    public int? FillB { get; set; }
    public int? BorderSize { get; set; }
    public int? BorderA { get; set; }
    public int? BorderR { get; set; }
    public int? BorderG { get; set; }
    public int? BorderB { get; set; }
    public int? BorderStyle { get; set; }
    public string FontName { get; set; }
    public int? FontA { get; set; }
    public int? FontR { get; set; }
    public int? FontG { get; set; }
    public int? FontB { get; set; }
    public int? FontSize { get; set; }
    public int? FontStyle { get; set; }
    public string Text { get; set; }
    public int ZOrder { get; set; }
    public bool DrawCrossLines { get; set; }
    public int MarkupSubType { get; set; }
    public decimal? Xd { get; set; }
    public decimal? Yd { get; set; }
    public decimal? WidthD { get; set; }
    public decimal? HeightD { get; set; }

    public ImportFileRecord(string documentIdentifier, string fileOrder, string x, string y, string width, string height, string markupType, string fillA, string fillR, string fillG, string fillB, string borderSize, string borderA, string borderR, string borderG, string borderB, string borderStyle, string fontName, string fontA, string fontR, string fontG, string fontB, string fontSize, string fontStyle, string text, string zOrder, string drawCrossLines, string markupSubType, string xD, string yD, string widthD, string heightD)
    {
      if (documentIdentifier == null || documentIdentifier == "null")
      {
        throw new MarkupUtilityException($"{nameof(DocumentIdentifier)} cannot be NULL.");
      }
      if (string.IsNullOrWhiteSpace(documentIdentifier))
      {
        throw new MarkupUtilityException($"{nameof(DocumentIdentifier)} is not valid.");
      }

      DocumentIdentifier = documentIdentifier;


      if (fileOrder == null || fileOrder == "null")
      {
        throw new MarkupUtilityException($"{nameof(FileOrder)} cannot be NULL.");
      }
      int fileOrderValue;
      if (!int.TryParse(fileOrder, out fileOrderValue))
      {
        throw new MarkupUtilityException($"{nameof(FileOrder)} is not a valid Integer.");
      }
      if (fileOrderValue < 0)
      {
        throw new MarkupUtilityException($"{nameof(FileOrder)} cannot be negative.");
      }

      FileOrder = fileOrderValue;


      if (x == null || x == "null")
      {
        throw new MarkupUtilityException($"{nameof(X)} cannot be NULL.");
      }
      int xValue;
      if (!int.TryParse(x, out xValue))
      {
        throw new MarkupUtilityException($"{nameof(X)} is not a valid Integer.");
      }
      if (xValue < 0)
      {
        throw new MarkupUtilityException($"{nameof(X)} cannot be negative.");
      }

      X = xValue;


      if (y == null || y == "null")
      {
        throw new MarkupUtilityException($"{nameof(Y)} cannot be NULL.");
      }
      int yValue;
      if (!int.TryParse(y, out yValue))
      {
        throw new MarkupUtilityException($"{nameof(Y)} is not a valid Integer.");
      }
      if (yValue < 0)
      {
        throw new MarkupUtilityException($"{nameof(Y)} cannot be negative.");
      }

      Y = yValue;


      if (width == null || width == "null")
      {
        throw new MarkupUtilityException($"{nameof(Width)} cannot be NULL.");
      }
      int widthValue;
      if (!int.TryParse(width, out widthValue))
      {
        throw new MarkupUtilityException($"{nameof(Width)} is not a valid Integer.");
      }

      Width = widthValue;


      if (height == null || height == "null")
      {
        throw new MarkupUtilityException($"{nameof(Height)} cannot be NULL.");
      }
      int heightValue;
      if (!int.TryParse(height, out heightValue))
      {
        throw new MarkupUtilityException($"{nameof(Height)} is not a valid Integer.");
      }

      Height = heightValue;


      if (markupType == null || markupType == "null")
      {
        throw new MarkupUtilityException($"{nameof(MarkupType)} cannot be NULL.");
      }
      int markupTypeValue;
      if (!int.TryParse(markupType, out markupTypeValue))
      {
        throw new MarkupUtilityException($"{nameof(MarkupType)} is not a valid Integer.");
      }
      if (markupTypeValue < 0)
      {
        throw new MarkupUtilityException($"{nameof(MarkupType)} cannot be negative.");
      }

      MarkupType = markupTypeValue;


      if (fillA == null || fillA == "null")
      {
        FillA = null;
      }
      else
      {
        int fillAValue;
        if (!int.TryParse(fillA, out fillAValue))
        {
          throw new MarkupUtilityException($"{nameof(FillA)} is not a valid Integer.");
        }
        if (fillAValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(FillA)} cannot be negative.");
        }
        FillA = fillAValue;
      }


      if (fillR == null || fillR == "null")
      {
        FillR = null;
      }
      else
      {
        int fillRValue;
        if (!int.TryParse(fillR, out fillRValue))
        {
          throw new MarkupUtilityException($"{nameof(FillR)} is not a valid Integer.");
        }
        if (fillRValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(FillR)} cannot be negative.");
        }
        FillR = fillRValue;
      }


      if (fillG == null || fillG == "null")
      {
        FillG = null;
      }
      else
      {
        int fillGValue;
        if (!int.TryParse(fillG, out fillGValue))
        {
          throw new MarkupUtilityException($"{nameof(FillG)} is not a valid Integer.");
        }
        if (fillGValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(FillG)} cannot be negative.");
        }
        FillG = fillGValue;
      }


      if (fillB == null || fillB == "null")
      {
        FillB = null;
      }
      else
      {
        int fillBValue;
        if (!int.TryParse(fillB, out fillBValue))
        {
          throw new MarkupUtilityException($"{nameof(FillB)} is not a valid Integer.");
        }
        if (fillBValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(FillB)} cannot be negative.");
        }
        FillB = fillBValue;
      }


      if (borderSize == null || borderSize == "null")
      {
        BorderSize = null;
      }
      else
      {
        int borderSizeValue;
        if (!int.TryParse(borderSize, out borderSizeValue))
        {
          throw new MarkupUtilityException($"{nameof(BorderSize)} is not a valid Integer.");
        }
        if (borderSizeValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(BorderSize)} cannot be negative.");
        }
        BorderSize = borderSizeValue;
      }


      if (borderA == null || borderA == "null")
      {
        BorderA = null;
      }
      else
      {
        int borderAValue;
        if (!int.TryParse(borderA, out borderAValue))
        {
          throw new MarkupUtilityException($"{nameof(BorderA)} is not a valid Integer.");
        }
        if (borderAValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(BorderA)} cannot be negative.");
        }
        BorderA = borderAValue;
      }


      if (borderR == null || borderR == "null")
      {
        BorderR = null;
      }
      else
      {
        int borderRValue;
        if (!int.TryParse(borderR, out borderRValue))
        {
          throw new MarkupUtilityException($"{nameof(BorderR)} is not a valid Integer.");
        }
        if (BorderR < 0)
        {
          throw new MarkupUtilityException($"{nameof(BorderR)} cannot be negative.");
        }
        BorderR = borderRValue;
      }


      if (borderG == null || borderG == "null")
      {
        BorderG = null;
      }
      else
      {
        int borderGValue;
        if (!int.TryParse(borderG, out borderGValue))
        {
          throw new MarkupUtilityException($"{nameof(BorderG)} is not a valid Integer.");
        }
        if (borderGValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(BorderG)} cannot be negative.");
        }
        BorderG = borderGValue;
      }


      if (borderB == null || borderB == "null")
      {
        BorderB = null;
      }
      else
      {
        int borderBValue;
        if (!int.TryParse(borderB, out borderBValue))
        {
          throw new MarkupUtilityException($"{nameof(BorderB)} is not a valid Integer.");
        }
        if (borderBValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(BorderB)} cannot be negative.");
        }
        BorderB = borderBValue;
      }


      if (fontName == null || fontName == "null")
      {
        FontName = null;
      }
      else
      {
        FontName = fontName;
      }


      if (borderStyle == null || borderStyle == "null")
      {
        BorderStyle = null;
      }
      else
      {
        int borderStyleValue;
        if (!int.TryParse(borderStyle, out borderStyleValue))
        {
          throw new MarkupUtilityException($"{nameof(BorderStyle)} is not a valid Integer.");
        }
        if (borderStyleValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(BorderStyle)} cannot be negative.");
        }
        BorderStyle = borderStyleValue;
      }


      if (fontA == null || fontA == "null")
      {
        FontA = null;
      }
      else
      {
        int fontAValue;
        if (!int.TryParse(fontA, out fontAValue))
        {
          throw new MarkupUtilityException($"{nameof(FontA)} is not a valid Integer.");
        }
        if (fontAValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(FontA)} cannot be negative.");
        }
        FontA = fontAValue;
      }


      if (fontR == null || fontR == "null")
      {
        FontR = null;
      }
      else
      {
        int fontRValue;
        if (!int.TryParse(fontR, out fontRValue))
        {
          throw new MarkupUtilityException($"{nameof(FontR)} is not a valid Integer.");
        }
        if (fontRValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(FontR)} cannot be negative.");
        }
        FontR = fontRValue;
      }


      if (fontG == null || fontG == "null")
      {
        FontG = null;
      }
      else
      {
        int fontGValue;
        if (!int.TryParse(fontG, out fontGValue))
        {
          throw new MarkupUtilityException($"{nameof(FontG)} is not a valid Integer.");
        }
        if (fontGValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(FontG)} cannot be negative.");
        }
        FontG = fontGValue;
      }


      if (fontB == null || fontB == "null")
      {
        FontB = null;
      }
      else
      {
        int fontBValue;
        if (!int.TryParse(fontB, out fontBValue))
        {
          throw new MarkupUtilityException($"{nameof(FontB)} is not a valid Integer.");
        }
        if (fontBValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(FontB)} cannot be negative.");
        }
        FontB = fontBValue;
      }


      if (fontSize == null || fontSize == "null")
      {
        FontSize = null;
      }
      else
      {
        int fontSizeValue;
        if (!int.TryParse(fontSize, out fontSizeValue))
        {
          throw new MarkupUtilityException($"{nameof(FontSize)} is not a valid Integer.");
        }
        FontSize = fontSizeValue;
      }


      if (fontStyle == null || fontStyle == "null")
      {
        FontStyle = null;
      }
      else
      {
        int fontStyleValue;
        if (!int.TryParse(fontStyle, out fontStyleValue))
        {
          throw new MarkupUtilityException($"{nameof(FontStyle)} is not a valid Integer.");
        }
        if (fontStyleValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(FontStyle)} cannot be negative.");
        }
        FontStyle = fontStyleValue;
      }


      if (text == null || text == "null")
      {
        Text = null;
      }
      else
      {
        Text = text;
      }


      if (zOrder == null || zOrder == "null")
      {
        throw new MarkupUtilityException($"{nameof(ZOrder)} cannot be NULL.");
      }
      else
      {
        int zOrderValue;
        if (!int.TryParse(zOrder, out zOrderValue))
        {
          throw new MarkupUtilityException($"{nameof(ZOrder)} is not a valid Integer.");
        }
        ZOrder = zOrderValue;
      }

      if (drawCrossLines == null || drawCrossLines == "null")
      {
        throw new MarkupUtilityException($"{nameof(DrawCrossLines)} cannot be NULL.");
      }
      int drawCrossLinesValue;
      if (!int.TryParse(drawCrossLines, out drawCrossLinesValue))
      {
        throw new MarkupUtilityException($"{nameof(DrawCrossLines)} is not a valid Integer.");
      }
      if (drawCrossLinesValue != 0 && drawCrossLinesValue != 1)
      {
        throw new MarkupUtilityException($"{nameof(DrawCrossLines)} valid values are 0 or 1.");
      }

      DrawCrossLines = (drawCrossLinesValue == 1);


      if (markupSubType == null || markupSubType == "null")
      {
        throw new MarkupUtilityException($"{nameof(MarkupSubType)} cannot be NULL.");
      }
      int markupSubTypeValue;
      if (!int.TryParse(markupSubType, out markupSubTypeValue))
      {
        throw new MarkupUtilityException($"{nameof(MarkupSubType)} is not a valid Integer.");
      }
      if (markupSubTypeValue < 0)
      {
        throw new MarkupUtilityException($"{nameof(MarkupSubType)} cannot be negative.");
      }

      MarkupSubType = markupSubTypeValue;

      if (xD == null || xD == "null")
      {
        Xd = null;
      }
      else
      {
        decimal xDvalue;
        if (!decimal.TryParse(xD, out xDvalue))
        {
          throw new MarkupUtilityException($"{nameof(Xd)} is not a valid Decimal.");
        }
        if (xDvalue < 0)
        {
          throw new MarkupUtilityException($"{nameof(Xd)} cannot be negative.");
        }

        Xd = xDvalue;
      }


      if (yD == null || yD == "null")
      {
        Yd = null;
      }
      else
      {
        decimal yDValue;
        if (!decimal.TryParse(yD, out yDValue))
        {
          throw new MarkupUtilityException($"{nameof(Yd)} is not a valid Decimal.");
        }
        if (yDValue < 0)
        {
          throw new MarkupUtilityException($"{nameof(Yd)} cannot be negative.");
        }

        Yd = yDValue;
      }


      if (widthD == null || widthD == "null")
      {
        WidthD = null;
      }
      else
      {
        decimal widthDValue;
        if (!decimal.TryParse(widthD, out widthDValue))
        {
          throw new MarkupUtilityException($"{nameof(WidthD)} is not a valid Decimal.");
        }

        WidthD = widthDValue;
      }


      if (heightD == null || heightD == "null")
      {
        HeightD = null;
      }
      else
      {
        decimal heightDValue;
        if (!decimal.TryParse(heightD, out heightDValue))
        {
          throw new MarkupUtilityException($"{nameof(HeightD)} is not a valid Decimal.");
        }

        HeightD = heightDValue;
      }
    }

    public override string ToString()
    {
      const string stringSeparator = ", ";
      var retVal = DocumentIdentifier + stringSeparator
                + FileOrder + stringSeparator
                + X + stringSeparator
                + Y + stringSeparator
                + Width + stringSeparator
                + Height + stringSeparator
                + MarkupType + stringSeparator
                + FillA + stringSeparator
                + FillR + stringSeparator
                + FillG + stringSeparator
                + FillB + stringSeparator
                + BorderSize + stringSeparator
                + BorderA + stringSeparator
                + BorderR + stringSeparator
                + BorderG + stringSeparator
                + BorderB + stringSeparator
                + BorderStyle + stringSeparator
                + FontName + stringSeparator
                + FontA + stringSeparator
                + FontR + stringSeparator
                + FontG + stringSeparator
                + FontB + stringSeparator
                + FontSize + stringSeparator
                + FontStyle + stringSeparator
                + Text + stringSeparator
                + ZOrder + stringSeparator
                + DrawCrossLines + stringSeparator
                + MarkupSubType + stringSeparator
                                      + Xd + stringSeparator
                                      + Yd + stringSeparator
                                      + WidthD + stringSeparator
                                      + HeightD + stringSeparator;

      return retVal;
    }
  }
}
