export interface GivenTemplate {
    id?: number,
    name?: string,
    promptGroups?: PromptGroup[],
    active?: boolean,
    title?: string,
    link?: PromptLink,
    saveButtonText?: string,
    cancelButtonText?: string
}
export interface PromptGroup {
    id: number,
    sequence?: number,
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
    placeholderText?: string,
    params?: string[],
    isOnNewline?: boolean,
    required?: boolean,
    promptChoices?: PromptChoice[],
    promptChildren?: number[],
    displayChildPromptsValue?: string,
    isActive?: boolean
}
export interface PromptChoice {
    id?: number,
    promptId?: number,
    sequence?: number,
    choiceText: string,
    isActive?: boolean
}

export interface PromptLink {
    href: string,
    rel?: string,
    method?: string
}