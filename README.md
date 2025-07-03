# ClockMate
2025 SWU Capstone Design Project

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

---

## PR 규칙
- **PR 제목**: 내가 작업한 내용을 한 문장으로 요약해서 작성
- 1개의 PR에는 1개의 핵심 기능 추가/변경만 포함

---

## 변수명 규칙
|이름|표기법|예시|
|:---:|:---:|:---:|
|변수|_camelCase|_spawnPoint, _isWalking|
|매개변수|camelCase|spawnPoint, isWalking|
|메서드명|PascalCase|SpawnPlayer()|
|클래스명|PascalCase|GameManager|
|상수|PascalCase|MaxHealth|

## 메인 가져오기
### 기존 변경사항 잠시 버려두기 (해당 하지 않으면 '메인 가져오기'로 ->)
⚠️가능하면 Changes가 없을 때 가져오는 것을 추천 (충돌이 날 수 있기 때문..)
간혹 변경 사항을 없앤 후 받아오라는 문구가 뜰 때가 있는데
![image (2)](https://github.com/user-attachments/assets/6f9fefd2-9556-4c1f-bb8a-17a08ad8ed04)
- 11 changed files 부분 우클릭 후 Stash all changes (잠시 버리기)
- ⚠️Discard all changes는 영구 삭제이므로 유의할 것!
- 이 상태에서 메인 가져오기(1번~마지막까지 진행)
![image (3)](https://github.com/user-attachments/assets/4a714f3a-6c51-4316-89e2-90773c8f26cd)
- 이후 View Stash를 누르면 임의로 버린 파일들을 볼 수 있음
![image (4)](https://github.com/user-attachments/assets/882d40fd-c6fe-4aa9-9171-f87a39a362d1)
- Restore 클릭 시 임시로 버렸던 변경사항들이 다시 돌아옴
- ⚠️Discard는 영구 삭제이므로 유의할 것!

### 메인 가져오기
![image (9)](https://github.com/user-attachments/assets/ca4955f0-fc6f-4caf-82d0-7e81d048a2da)

![image (5)](https://github.com/user-attachments/assets/ccedcd3d-cbd1-43a5-aa76-169dbe13839f)

![image (6)](https://github.com/user-attachments/assets/122724ff-2e6e-4131-8e30-86987724b07b)
- main 선택 후 ‘Create a merge commit’ 클릭

![image (7)](https://github.com/user-attachments/assets/eeb8d221-4c9f-4f72-801e-00f19d4a84a6)
