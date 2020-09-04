export interface ModalHeaderParameters {
  label?: string;
  title?: string;
  class?: string[];
  toolTip?: string;
  onTitleClick?: any;
  buttons?: Array<ModalHeaderButtons>;
  fields?: Array<ModalHeaderParameterField>;
}

interface ModalHeaderButtons {
  id?: string;
  name?: string;
  onClick?: any;
  toolTip?: string;
}

interface ModalHeaderParameterField {
  id?: string;
  label?: string;
  value?: string;
  onClick?: any;
  toolTip?: string;
}
