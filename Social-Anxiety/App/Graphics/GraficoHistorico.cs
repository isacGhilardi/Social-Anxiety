using Microsoft.Maui.Graphics;
using Social_Anxiety.Core.CheckIns;

namespace Social_Anxiety;

public sealed class GraficoHistorico(ChartSeries serie) : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (serie.IsEmpty) return;
        const float esquerda = 32;
        const float topo = 12;
        float largura = dirtyRect.Width - esquerda - 28;
        float altura = dirtyRect.Height - topo - 32;
        if (largura <= 0 || altura <= 0) return;

        var recursos = Application.Current!.Resources;
        bool escuro = Application.Current.RequestedTheme == AppTheme.Dark;
        canvas.FontColor = (Color)recursos[escuro ? "CalmeMutedDark" : "CalmeMuted"];
        // CalmeTrack some no fundo (contraste 1,2:1); CalmeBorder deixa a grade visível nos dois temas.
        Color grade = (Color)recursos[escuro ? "CalmeBorderDark" : "CalmeBorder"];
        Color destaque = (Color)recursos[escuro ? "CalmePrimaryDark" : "CalmePrimary"];
        canvas.FontSize = 12;
        for (int nivel = 1; nivel <= 5; nivel++)
        {
            float y = topo + altura - (nivel - 1) * altura / 4;
            canvas.StrokeColor = grade;
            canvas.StrokeSize = 1;
            canvas.DrawLine(esquerda, y, esquerda + largura, y);
            canvas.DrawString(nivel.ToString(), 4, y + 4, Microsoft.Maui.Graphics.HorizontalAlignment.Left);
        }

        float distancia = serie.Points.Count > 1 ? largura / (serie.Points.Count - 1) : 0;
        PointF? anterior = null;
        canvas.StrokeColor = destaque;
        canvas.FillColor = destaque;
        canvas.StrokeSize = 3;
        for (int i = 0; i < serie.Points.Count; i++)
        {
            var ponto = serie.Points[i];
            float x = esquerda + (serie.Points.Count == 1 ? largura / 2 : i * distancia);
            canvas.DrawString(ponto.Label, x, topo + altura + 22, Microsoft.Maui.Graphics.HorizontalAlignment.Center);
            // Um intervalo sem registro mantém seu espaço e interrompe a linha.
            if (ponto.Value is not double valor)
            {
                anterior = null;
                continue;
            }
            float y = topo + altura - (float)(valor - 1) * altura / 4;
            if (anterior is PointF ultimo) canvas.DrawLine(ultimo.X, ultimo.Y, x, y);
            canvas.FillCircle(x, y, 5);
            anterior = new PointF(x, y);
        }
    }
}
