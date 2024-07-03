# 2024CardGame
 2024CardGame by Unity
 
### 결과물(영상) : https://youtu.be/CnPrfhej7yo?si=qPfsK75C2_ZNpwj_
[![Video Label](http://img.youtube.com/vi/CnPrfhej7yo/0.jpg)](https://www.youtube.com/watch?v=CnPrfhej7yo)

### - Photon Fusion사용
- - Shared 모드를 사용해 중개 서버에서 호스트를 책임짐
  - RPC(remote procedure call)를 사용해 클라이언트간 동일한 동작 실행

### - 하스트톤의 기능들을 구현
- - 전투의 함성
  - 죽음의 메아리
  - 마법카드
  - 하수인
  - 영웅 능력
  - 코스트 시스템

### - 시행착오
카드를 연속으로 내는 경우 애니메이션이 하나씩 실행되어야 했습니다. 
또한 애니메이션과는 별개로 연속으로 내면 그 즉시 카드의 효과를 수행해야했습니다. 
그래서 효과는 호출시 바로 적용시키고 애니메이션의 경우 버퍼를 사용해 순차적으로 실행되게  딜레이 시켰습니다. 

직접 구현을 하기 전에는 이러한 과정이 필요하다고 생각하지 못했습니다. 
시행착오가 있었지만 결과적으로 버퍼를 사용해 문제를 해결했습니다. 
앞으로 이런 비슷한 상황에 놓이면 더욱 빠르고 유연하게 문제를 해결할 수 있을 것이라 생각합니다. 
