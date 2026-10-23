using Social_Anxiety.Core.Auth;

namespace Social_Anxiety;

/// <summary>Tela "Seu espaço" como raiz do Shell, para quem já passou pelo onboarding e está sem sessão.</summary>
public sealed class AccessPage(OnboardingState state, SessionState session) : OnboardingPage(state, session, "access");
