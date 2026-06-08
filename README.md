# New_Attenuator

안테나 감쇠기 제어용 Windows Forms 프로그램이다.

## 개요

- 시리얼 포트를 통해 감쇠기 장비를 제어한다
- AP 채널별 감쇠 값을 수동 또는 자동으로 조정한다
- 로밍 및 handover 테스트용 모드를 제공한다
- 설정값은 INI 파일로 저장하고 불러올 수 있다

## 장비 정보

- 감쇠기 모델: `8321-M6-12-TS`
- API: `Weinschel`
- GAGE CODE: `93459`
- 채널 수: `12`
- 감쇠 범위: `95.75 / 0.2dB steps`
- AP 모델: `AIR-AP3802E-k-k9`

## 시리얼 포트 연결

프로그램은 장치의 COM 포트를 통해 통신한다.

- 포트 선택: UI의 `Com Port` 콤보박스에서 선택
- 연결: `Connection` 버튼으로 연결/해제
- 사용 가능한 포트는 실행 시 자동으로 감지한다
- `Launch Virtual Serial Port Driver`를 사용하면 시리얼 포트 통신을 미러링해서 전달 값을 쉽게 확인할 수 있다
- 실제 장비 없이도 전송되는 명령어를 검증하거나 테스트 로그를 확인할 때 유용하다

시리얼 통신 설정은 다음과 같다.

- Baud rate: `115200`
- Data bits: `8`
- Stop bits: `1`
- Parity: `None`
- Handshake: `None`
- Read timeout: `500 ms`

## 제어 명령어

감쇠기 제어는 텍스트 명령을 시리얼 포트로 전송하는 방식이다.

### 1. 감쇠 설정

형식:

```text
ATTN {channel} {value}
```

예:

```text
ATTN 1 20
ATTN 4 40
ATTN ALL 10
```

의미:

- `channel`: 제어할 채널 번호 또는 `ALL`
- `value`: 감쇠 값
- `ALL`: 모든 채널을 한 번에 제어할 때 사용

### 2. 명령 완료 확인

감쇠 설정 후 확인 명령을 추가로 보낸다.

```text
*OPC?
```

이 명령은 장비가 이전 명령 처리를 끝냈는지 확인하는 용도다.

### 3. 실제 동작 흐름

프로그램은 다음 순서로 동작한다.

1. `ATTN {channel} {value}` 전송
2. `*OPC?` 전송
3. 응답 대기
4. 타임아웃 시 다음 동작 진행

## 문서

- 모드 설명: [Mode_Descriptions.md](./New_Attenuator/Mode_Descriptions.md)

## 버전

이 프로젝트는 `Semantic Versioning` 형식인 `MAJOR.MINOR.PATCH`로 버전을 관리한다.

- `PATCH` 예: `1.0.1` - 버그 수정
- `MINOR` 예: `1.1.0` - 기능 추가
- `MAJOR` 예: `2.0.0` - 배포 기준이 바뀌는 큰 변경

운영 기준은 다음과 같다.

- `0.x.x`: 개발 및 검증 단계
- `1.0.0`: 외부 배포가 가능한 안정판
- 버그만 수정하면 `1.0.1`, `1.0.2`처럼 `PATCH`를 올린다
- 기능이 추가되면 `1.1.0`, `1.2.0`처럼 `MINOR`를 올린다
- 큰 구조 변경이나 배포 기준 도달 시 `1.0.0` 또는 `2.0.0`으로 올린다

현재 프로그램 버전은 `1.0.0`이다.

## 버전 표시 방식

프로그램 타이틀에는 `New_Attenuator 1.0.0` 형태로 버전을 표시한다.

버전 문자열 뒤에 `+커밋해시` 같은 추가 정보는 표시하지 않는다.

## 설정 파일

- 설정 파일은 `INI` 형식으로 저장한다
- 파일명 예: `attenuator_setting_YYYYMMDDHHmm.ini`
- 프로그램 설정과 장비 상태를 함께 저장하고 불러올 수 있다

## 실행 파일

- 프로젝트: `New_Attenuator`
- 대상 프레임워크: `net8.0-windows`
- UI: Windows Forms

## 참고

세부 동작은 [Mode_Descriptions.md](./New_Attenuator/Mode_Descriptions.md)를 참고한다.
