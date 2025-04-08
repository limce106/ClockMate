# TowerOfTime

---

## 깃허브 커밋 규칙
- Ref: [Git Commit Message Convention](https://github.com/gyoogle/tech-interview-for-developer/blob/master/ETC/Git%20Commit%20Message%20Convention.md)

**커밋 메세지 형식**
> type: Subject (제목)
> <br/>
> body (본문)
> <br/>
> footer (꼬리말)

- `feat` : 새로운 기능에 대한 커밋
- `fix` : 버그 수정에 대한 커밋
- `build` : 빌드 관련 파일 수정에 대한 커밋
- `chore` : 그 외 자잘한 수정에 대한 커밋
- `ci` : CI관련 설정 수정에 대한 커밋
- `docs` : 문서 수정에 대한 커밋
- `style` : 코드 스타일 혹은 포맷 등에 관한 커밋
- `refactor` : 코드 리팩토링에 대한 커밋
- `test` : 테스트 코드 수정에 대한 커밋

**Subject (제목)**

- *한글*로 간결하게 작성

**Body (본문)**

- 상세히 작성, 기본적으로 무엇을 왜 진행 하였는지 작성
- Issue 등록 시, Issue 태그

**footer (꼬리말)**

- 참고사항

<br/>

---

## PR 규칙
- **PR 제목**: 내가 작업한 내용을 한 문장으로 요약해서 작성
- 1개의 PR에는 1개의 핵심 기능 추가/변경만 포함

<br/>

---

## 변수명 규칙
|이름|표기법|예시|
|:---:|:---:|:---:|
|변수|camelCase|spawnPoint, isWalking|
|메서드명|PascalCase|SpawnPlayer()|
|클래스명|PascalCase|GameManager|
|상수|UPPER_SNAKE_CASE|MAX_HEALTH|
