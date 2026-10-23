using Social_Anxiety;

namespace Calme.Core.Tests;

public class BreathingSessionTests
{
    [Fact]
    public void Percorre_inspirar_segurar_e_expirar_nos_limites_corretos()
    {
        var clock = new TestClock();
        var session = new BreathingSession(clock);
        session.Start();
        Assert.Equal(BreathingPhase.Inhale, session.Phase); Assert.Equal(4, session.SecondsRemaining);
        clock.Advance(4); Assert.Equal(BreathingPhase.Hold, session.Phase); Assert.Equal(2, session.SecondsRemaining); Assert.Equal(1, session.Scale, 3);
        clock.Advance(2); Assert.Equal(BreathingPhase.Exhale, session.Phase); Assert.Equal(6, session.SecondsRemaining);
        clock.Advance(6); Assert.Equal(BreathingPhase.Inhale, session.Phase); Assert.Equal(0.64, session.Scale, 3);
    }

    [Fact]
    public void Animacao_tem_interpolacao_suave_no_meio_das_fases()
    {
        var clock = new TestClock(); var session = new BreathingSession(clock);
        session.Start(); clock.Advance(2); Assert.Equal(0.82, session.Scale, 3);
        clock.Advance(7); Assert.Equal(0.82, session.Scale, 3);
    }

    [Fact]
    public void Pausar_congela_o_tempo_e_continuar_retoma_do_mesmo_ponto()
    {
        var clock = new TestClock(); var session = new BreathingSession(clock);
        session.Start(); clock.Advance(3); session.Pause(); clock.Advance(20);
        Assert.Equal(1, session.SecondsRemaining); Assert.Equal(3, session.Elapsed.TotalSeconds, 3);
        session.Start(); clock.Advance(1); Assert.Equal(BreathingPhase.Hold, session.Phase);
    }

    [Fact]
    public void Sem_segurar_a_inspiracao_vai_direto_para_expiracao_e_o_ciclo_dura_dez_segundos()
    {
        var clock = new TestClock(); var session = new BreathingSession(clock) { SkipHold = true };
        session.Start(); clock.Advance(4); Assert.Equal(BreathingPhase.Exhale, session.Phase);
        clock.Advance(6); Assert.Equal(BreathingPhase.Inhale, session.Phase);
    }

    [Fact]
    public void Reiniciar_zera_o_ciclo_e_exige_novo_inicio()
    {
        var clock = new TestClock(); var session = new BreathingSession(clock);
        session.Start(); clock.Advance(8); session.Reset();
        Assert.False(session.IsRunning); Assert.False(session.HasStarted); Assert.Equal(0, session.Elapsed.TotalSeconds, 3);
    }

    [Fact]
    public void Atraso_de_quadros_nao_acumula_erro_no_ciclo()
    {
        var clock = new TestClock(); var session = new BreathingSession(clock);
        session.Start(); clock.Advance(1202); Assert.Equal(BreathingPhase.Inhale, session.Phase); Assert.Equal(2, session.SecondsRemaining);
    }
}
