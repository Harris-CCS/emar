export interface GivenTemplate {
    id: number,
    name: string,
    promptGroups: PromptGroup[],
    active?: boolean,
    title?: string,
    siteId?: number
}
export interface PromptGroup {
    id: number,
    name?: string,
    displayTitle?: string,
    prompts: Prompt[],
    siteId?: number
}
export interface Prompt {
    id: number,
    type: string, // CheckBox, DropDownListBox, FreeText, MultiLineFreeText, Date, User
    promptGroupId?: number,
    sequence?: number,
    prompt?: string,
    default?: string,
    required?: boolean,
    promptChoices?: PromptChoice[],
    isActive?: boolean
}
export interface PromptChoice {
    id: number,
    promptId?: number,
    sequence?: number,
    choiceText: string,
    isActive?: boolean
}