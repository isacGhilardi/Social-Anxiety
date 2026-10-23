using Microsoft.Maui.Graphics;
using Social_Anxiety.Core.CheckIns;

namespace Social_Anxiety;

public class ResumoGrafico : IDrawable
{
    private readonly CheckIn[] valores;

    public ResumoGrafico(CheckIn[] registros)
    {
        valores = registros;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (valores.Length == 0)
            return;

        float esquerda = 28;
        float topo = 12;
        float largura = dirtyRect.Width - esquerda - 16;
        float altura = dirtyRect.Height - topo - 28;

        if (largura <= 0 || altura <= 0)
            return;

        bool temaEscuro =
    Application.Current?.RequestedTheme == AppTheme.Dark;

        var recursos = Application.Current!.Resources;

        Color corTexto = (Color)recursos[
            temaEscuro ? "CalmeMutedDark" : "CalmeMuted"];

        Color corGrade = (Color)recursos[
            temaEscuro ? "CalmeTrackDark" : "CalmeTrack"];

        Color corLinha = (Color)recursos[
            temaEscuro ? "CalmePrimaryDark" : "CalmePrimary"];

        canvas.FontSize = 12;
        canvas.FontColor = corTexto;


        for (int nivel = 1; nivel <= 5; nivel++)
        {
            float y = topo + altura - (nivel - 1) * altura / 4;

            canvas.StrokeColor = corGrade;
            canvas.StrokeSize = 1;
            canvas.DrawLine(esquerda, y, esquerda + largura, y);

            canvas.DrawString(
                nivel.ToString(),
                4,
                y + 4,
                Microsoft.Maui.Graphics.HorizontalAlignment.Left);
        }

        float distancia = valores.Length > 1
            ? largura / (valores.Length - 1)
            : 0;

        canvas.StrokeColor = corLinha;
        canvas.FillColor = corLinha;
        canvas.StrokeSize = 3;

        for (int i = 0; i < valores.Length; i++)
        {
            float x = valores.Length == 1
                ? esquerda + largura / 2
                : esquerda + i * distancia;
            float y = topo + altura - (valores[i].Score - 1) * altura / 4;

            if (i > 0)
            {
                float xAnterior = esquerda + (i - 1) * distancia;
                float yAnterior =
                    topo + altura - (valores[i - 1].Score - 1) * altura / 4;

                canvas.DrawLine(xAnterior, yAnterior, x, y);
            }

            canvas.FillCircle(x, y, 4);

            if (i == 0 || i == valores.Length - 1 || i % Math.Max(1, valores.Length / 4) == 0)
                canvas.DrawString(
                valores[i].LocalDay.ToString("dd/MM"),
                x,
                topo + altura + 20,
                Microsoft.Maui.Graphics.HorizontalAlignment.Center);
        }
    }
}
