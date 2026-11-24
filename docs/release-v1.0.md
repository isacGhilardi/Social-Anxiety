# Release v1.0

Notas das versões 1.0.x do Calme para Android e Windows.

## Assinatura do APK

- O APK é assinado com o keystore de debug do SDK Android (`%LOCALAPPDATA%\Xamarin\Mono for Android\debug.keystore`, alias `androiddebugkey`), o mesmo usado na 1.0 e na 1.0.1. Certificado SHA-256: `a39fccebab2859433af5e2d53b0de3ef13575787c28f1083ba2def6f9fb9cab8`.
- Para a atualização instalar por cima da versão anterior, a nova versão precisa da mesma chave. Com outra chave, o Android recusa a atualização e é preciso desinstalar o app, o que apaga os dados locais.
- A senha vem só da variável de ambiente `CALME_KEY_PASS`. Ela não fica no repositório.
- Limitação: a chave de debug serve para a entrega acadêmica e para instalar fora da loja. Para publicar na Play Store, é preciso criar um keystore próprio, e os aparelhos com a 1.0.x terão de reinstalar o app.

Comando:

```
dotnet publish Social-Anxiety/Social-Anxiety.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk -p:AndroidKeyStore=true -p:AndroidSigningKeyStore="%LOCALAPPDATA%\Xamarin\Mono for Android\debug.keystore" -p:AndroidSigningKeyAlias=androiddebugkey -p:AndroidSigningKeyPass=env:CALME_KEY_PASS -p:AndroidSigningStorePass=env:CALME_KEY_PASS
```

## 1.0.2 (24/11)

`versionName` 1.0.2, `versionCode` 3. Instala por cima da 1.0.1 (`versionCode` 2): a assinatura é a mesma e os dados locais ficam preservados.

O que mudou:

- **Interface:** tema escuro com tokens novos (`CalmeAction` no fundo dos botões) e hierarquia de botões com box (`CalmeButton`, `CalmeSecondaryButton`, `CalmeLinkButton`, `CalmeOverlayButton`).
- **Barra inferior:** ícones e um indicador que desliza entre as abas. Ele respeita "Reduzir movimentos", e a troca de aba não tem mais a animação de empilhar.
- **Telas:** Início em grade 2×3 e Técnicas em cartões. "Voltar ao início" saiu de Histórico, Desafios e Perfil; a volta é pela aba Início.
- **Login e Cadastro:** campos com box, sem borda dupla.
- **Respiração no Windows:** a prática não pausa mais quando a janela só perde o foco. Minimizar continua pausando.
- **Desafios no tema escuro:** o nível bloqueado fica recuado, e não mais claro que o disponível.

Arquivos gerados (pasta `artifacts/`, fora do git):

- `artifacts/v1.0.2/Calme-1.0.2.apk`
- `artifacts/v1.0.2/windows/Social-Anxiety.exe`, executável sem instalador para o notebook da apresentação.
