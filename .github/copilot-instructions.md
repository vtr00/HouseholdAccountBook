# Copilot Instructions

## General Guidelines
- All review comments must be written in Japanese. Never answer in English.
- Provide specific and direct differences for the specified targets from the first response. General statements are unnecessary.
- Treat read-only investigation, static analysis, and result organization as tasks that can be performed within the Plan because they do not modify code.
- When the user instructs you to perform a check, investigation, or analysis, perform it immediately using available read-only means instead of ending with an explanation.
- When determining the source of a defect, do not judge from the current code alone; check the diff or history against the specified baseline branch.
- If the diff against the baseline branch cannot be checked, do not speculate about or assert the source of the defect. Clearly separate facts confirmed in the current code from matters not confirmed as the source of the defect.
- When the user specifies an output format, prioritize that format. When raw `.md` text is requested, present it in a Markdown code block.
- When the user specifies that the content must not be changed, present the wording, judgments, columns, and rows without modification.
- When an approved plan exists, perform read-only work within the Plan without requesting additional confirmation.
- If the requested work cannot be performed, first consider available alternatives and do not end with an explanation of the reason alone.
