"""Authoritative, deterministic schema source. Run before validate.py after edits."""
import json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
URI='https://schemas.excelapipoc.local/positioned/v2/'
S={'type':'string','minLength':1}; N={'type':'number'}; B={'type':'boolean'}; I={'type':'integer','minimum':0}; P={'type':'number','exclusiveMinimum':0}
def const(v):return {'const':v}
def enum(*v):return {'enum':list(v)}
def arr(s,minimum=0):return {'type':'array','items':s,'minItems':minimum}
def obj(p,optional=()):return {'type':'object','properties':p,'required':[k for k in p if k not in optional],'additionalProperties':False}
def ref(n):return {'$ref':'common.schema.json#/$defs/'+n}
def tup(s,n):return {'type':'array','items':s,'minItems':n,'maxItems':n}
def nullable(s):return {'anyOf':[s,{'type':'null'}]}
D={}
D['reference']=obj({'id':S,'version':S})
D['source']=obj({'file':S,'sha256':{'type':'string','pattern':'^[a-f0-9]{64}$'},'line':{'type':'integer','minimum':1},'rawCommand':S,'expression':S,'notes':arr(S)},('sha256','line','rawCommand','expression','notes'))
D['rect']=tup(N,4);D['point']=tup(N,2)
D['pageScope']=obj({'selection':enum('all','first','last','indices'),'indices':arr({'type':'integer','minimum':1}),'maximumPages':{'type':'integer','minimum':1}})
D['textMatch']=obj({'mode':enum('exact','contains','regex'),'value':S,'ignoreCase':B,'whitespace':enum('preserve','collapse','remove'),'unicodeNormalization':const('NFC'),'regexTimeoutMs':{'type':'integer','minimum':1}})
commonloc={'pages':ref('pageScope'),'assembly':enum('readingOrder','baseline','glyphClusters'),'cardinality':enum('one','zeroOrOne','many')}
D['locator']={'oneOf':[
 obj(dict(commonloc,kind=const('normalizedPage'),regionFraction=tup({'type':'number','minimum':0,'maximum':1},4))),
 obj(dict(commonloc,kind=const('calibratedRegion'),variantRef=S,frame=const('nominalPage'),rectPt=ref('rect'),paddingPt=I)),
 obj(dict(commonloc,kind=const('blockField'),variantRef=S,blockRef=S,fieldRef=S,frame=enum('nominalPage','blockLocal'))),
 obj(dict(commonloc,kind=const('relativeToAnchor'),variantRef=S,anchorRef=S,frame=const('nominalPage'),offsetRectPt=ref('rect')))
]}
D['selector']=obj({'semanticClasses':arr(S),'aggregationMemberships':arr(S),'level':enum('detail','analytic','synthetic','class','document','any'),'occurrences':enum('records','childGroups')})
D['scope']=obj({'kind':enum('document','section','group','record'),'groupRef':nullable(S)})
D['operand']={'oneOf':[obj({'kind':const('field'),'semanticName':S}),obj({'kind':const('calculation'),'ruleRef':S}),obj({'kind':const('literal'),'decimal':{'type':'string','pattern':'^-?[0-9]+(\\.[0-9]+)?$'}})]}
D['predicate']={'oneOf':[
 obj({'op':enum('all','any'),'conditions':arr(ref('predicate'),1)}),obj({'op':const('not'),'condition':ref('predicate')}),
 obj({'op':const('fieldState'),'fieldRef':S,'states':arr(enum('present','blank','unresolved','invalid','ambiguous'),1)}),
 obj({'op':const('textMatch'),'fieldRef':S,'match':ref('textMatch')}),
 obj({'op':const('blockKind'),'blockRefs':arr(S,1)}),
 obj({'op':const('groupContext'),'groupRef':S,'state':enum('open','closed','incomplete')})]}
rulebase={'id':S,'enabled':B,'evidenceStatus':enum('sourceDerived','sampleVerified','unverified'),'sourceRefs':arr(ref('source')),'reviewNotes':arr(S)}
def rule(op,props):return obj(dict(rulebase,op=const(op),**props))
D['calibration']=rule('fitUniformScaleTranslation',{'anchorRefs':arr(S,2),'minimumInliers':{'type':'integer','minimum':2},'minimumAnchorSeparationPt':P,'minimumSpanPt':tup(I,2),'scaleRange':tup(P,2),'maximumResidualPt':P,'maximumHypotheses':{'type':'integer','minimum':1},'onFailure':const('retainUnresolvedPage'),'inferScaleFromPageSize':const(False)})
D['anchor']=obj({'id':S,'feature':const('rulingEndpoint'),'pointPt':ref('point'),'blockRef':S,'lineRef':S,'endpointIndex':enum(0,1),'evidenceStatus':enum('sourceDerived','sampleVerified','unverified'),'sourceRefs':arr(ref('source'))})
D['field']=obj({'id':S,'semanticName':S,'rectPt':ref('rect'),'parserRef':S,'sourceRefs':arr(ref('source'))})
D['label']=obj({'id':S,'text':{'type':'string'},'rectPt':ref('rect'),'sourceRefs':arr(ref('source'))})
D['line']=obj({'id':S,'pointsPt':tup(ref('point'),2),'sourceRefs':arr(ref('source'))})
D['locateBlock']=rule('locateBlock',{'frame':enum('nominalPage','blockLocal'),'origin':enum('pageCalibration','independentOccurrence'),'signals':arr(enum('rulings','labels','fieldColumns','unassignedText'),1),'combination':const('union'),'onAmbiguous':const('retainHypotheses')})
D['detectCandidates']=rule('detectCandidates',{'signals':arr(enum('rulings','labels','fieldColumns','unassignedText'),1),'combination':const('union'),'requiresSuccessfulParsing':const(False)})
D['assignFields']=rule('assignFields',{'paddingPt':{'type':'number','minimum':0},'assembly':const('glyphClusters'),'splitAcrossFields':const('requireGlyphGeometry'),'overlap':const('retainAmbiguity'),'retainClippedSource':const(True)})
D['block']=obj({'id':S,'kind':enum('pageHeader','repeatingRecord','groupHeader','groupFooter','reportFooter','decoration'),'frame':enum('nominalPage','blockLocal'),'sourceRefs':arr(ref('source')),'nominalAdvancePt':nullable(P),'emptyTemplate':B,'placement':ref('locateBlock'),'candidateDetection':ref('detectCandidates'),'fieldAssignment':ref('assignFields'),'fields':arr(ref('field')),'labels':arr(ref('label')),'lines':arr(ref('line'))})
D['variant']=obj({'id':S,'evidenceStatus':enum('sourceDerived','sampleVerified','unverified'),'sourceRefs':arr(ref('source'),1),'nominalPageSizePt':tup(P,2),'anchors':arr(ref('anchor'),2),'calibration':ref('calibration'),'blocks':arr(ref('block'),1)})
D['metadata']=obj({'id':S,'semanticName':S,'locator':ref('locator'),'parserRef':S,'onAbsent':const('unresolved'),'onConflict':const('ambiguous'),'sourceRefs':arr(ref('source'))})
D['classification']=rule('classify',{'when':ref('predicate'),'semanticClass':S,'exportEligibility':enum('eligible','excluded','review'),'aggregationMemberships':arr(S),'level':enum('detail','analytic','synthetic','class','document','any')})
D['boundary']={'oneOf':[obj({'op':enum('sectionStart','sectionEnd','afterPreviousClosure')}),obj({'op':const('match'),'when':ref('predicate')})]}
D['group']=obj({'id':S,'parentRef':nullable(S),'memberSelector':ref('selector'),'startRule':ref('boundary'),'endRule':ref('boundary'),'keyFields':arr(S),'crossPagePolicy':const('requireContinuityEvidence'),'partialInputPolicy':const('retainIncomplete'),'repeatedHeaderResets':const(False),'inheritance':arr(obj({'fieldRef':S,'from':const('closingOccurrence'),'when':const('absent'),'onConflict':const('retainBothAndDiagnose')}))})
D['completeness']={'oneOf':[
 rule('requireFields',{'selector':ref('selector'),'fieldRefs':arr(S,1),'requiredState':const('present')}),
 rule('requireAny',{'selector':ref('selector'),'fieldRefs':arr(S,1),'requiredState':const('present')}),
 rule('accountForCandidates',{'onUnrecognized':const('preventCleanSuccess')}),
 rule('accountForTokens',{'decorationRequiresRuleId':const(True)}),
 rule('inspectCoverage',{'printedPageField':nullable(S),'contiguousPagesProveOriginalComplete':const(False)}),
 rule('checkSpacing',{'excludeExplainedBlocks':const(True),'maximumAdvanceMultiplier':P,'onGap':const('suspectedMissingRecord')}),
 rule('sequence',{'fieldRef':S,'scope':ref('scope'),'increment':{'type':'integer','minimum':1},'onGap':const('diagnosticOnly')})]}
calc={'scope':ref('scope'),'memberSelector':ref('selector'),'inputRefs':arr(ref('operand')),'prerequisites':arr(enum('resolvedClassification','resolvedGroupMembership','unambiguousOperands','completeScope')),'onMissingInput':const('inconclusive')}
D['calculation']={'oneOf':[rule('sum',dict(calc,inputRefs=arr(ref('operand'),1))),rule('count',dict(calc,inputRefs=tup(ref('operand'),0),mode=enum('occurrences','distinct'),distinctKey=arr(S))),rule('add',dict(calc,inputRefs=arr(ref('operand'),2))),rule('subtract',dict(calc,inputRefs=tup(ref('operand'),2)))]}
D['control']=obj({'id':S,'variantRef':S,'blockRef':S,'fieldRef':S,'scope':ref('scope')})
D['validationRule']={'oneOf':[rule('compare',{'scope':ref('scope'),'calculatedRef':S,'printedControlRef':S,'absoluteTolerance':{'type':'string','pattern':'^[0-9]+(\\.[0-9]+)?$'},'coverageRequirement':enum('completeScope','completeDocument'),'onMissingInput':const('inconclusive'),'onPartialCoverage':const('notEvaluated')}),rule('dateRange',{'scope':ref('scope'),'memberSelector':ref('selector'),'fieldRef':S,'startMetadataRef':S,'endMetadataRef':S,'onMissingInput':const('notEvaluated')}),rule('periodCodeRange',{'scope':ref('scope'),'fieldRef':S,'minimumCode':I,'maximumCode':I,'onUnknown':const('retainWithDiagnostic')})]}
D['parser']={'oneOf':[
 rule('text',{'trim':B,'normalizeNbsp':B}),
 rule('identifier',{'trimOuterWhitespace':B,'unicodeNormalization':const('NFC'),'preserveLeadingZeros':const(True),'preserveInternalSpaces':const(True),'allowLettersAndDiacritics':const(True)}),
 rule('integer',{'allowGrouping':B,'allowTrailingDot':B}),
 rule('exactDate',{'formats':arr(S,1),'twoDigitYear':const('uniqueExplicitContextOrAmbiguous'),'repairInvalidDate':const(False)}),
 rule('exactTime',{'formats':arr(S,1)}),
 rule('accountingPeriod',{'pattern':S,'codeGroup':S,'yearGroup':S,'observedCodes':arr(S),'unknownCode':const('retainWithDiagnostic'),'calendarDateConversion':const(False)}),
 rule('decimal',{'formats':arr(obj({'decimalSeparator':enum(',','.'),'groupingSeparators':arr(S),'wholeAmountSuffix':nullable(S)}),1),'groupingSize':const(3),'oneGroupingConventionPerValue':const(True),'allowLeadingSign':B,'allowSpaceAfterSign':B,'fractionDigits':I,'blankState':const('blank'),'distinctSuccessfulValues':const('ambiguous')})]}
# A comparison can also compare two explicit calculations (e.g. split debit/credit net).
D['validationRule']['oneOf'].append(rule('compare',{'scope':ref('scope'),'calculatedRef':S,'otherCalculatedRef':S,'absoluteTolerance':{'type':'string','pattern':'^[0-9]+(\\.[0-9]+)?$'},'coverageRequirement':enum('completeScope','completeDocument'),'onMissingInput':const('inconclusive'),'onPartialCoverage':const('notEvaluated')}))
D['metadataDerivation']=rule('deriveFiscalYear',{'inputMetadataNames':tup(S,2),'outputMetadataName':const('fiscalYear'),'onDifferentYears':const('unresolved'),'onMissingInput':const('unresolved'),'origin':const('derived')})
signalcommon={'locator':ref('locator'),'target':enum('category','producer','layout'),'candidate':S,'weight':P,'evidenceGroup':S,'polarity':enum('support','contradiction'),'missing':const('neutral')}
D['signal']={'oneOf':[
 rule('textMatch',dict(signalcommon,match=ref('textMatch'))),rule('markerMatch',dict(signalcommon,match=ref('textMatch'))),
 rule('orderedLabels',dict(signalcommon,labels=arr(ref('textMatch'),2),axis=enum('x','y'),maximumOrthogonalGapPt=P)),
 rule('relativePositions',dict(signalcommon,pairs=arr(obj({'first':ref('textMatch'),'second':ref('textMatch'),'deltaPt':ref('point'),'tolerancePt':P}),1))),
 rule('geometryFit',dict(signalcommon,layoutRef=ref('reference'),variantRefs=arr(S,1))) ]}
D['decision']=obj({'target':enum('category','producer','layout'),'candidate':S,'minimumScore':P,'minimumLead':{'type':'number','minimum':0},'requiredSignals':arr(S),'vetoSignals':arr(S),'requiredDecisions':arr(obj({'target':enum('category','producer'),'candidate':S}))})
base={'$schema':S,'schemaVersion':const(2),'definitionVersion':S,'id':S,'kind':S,'status':enum('reviewDraft','approved'),'productionApproved':B,'requiresCapabilities':arr(S),'sourceRefs':arr(ref('source')),'reviewNotes':arr(S)}
def doc(kind,p):return obj(dict(base,kind=const(kind),**p))
schemas={}
schemas['layout']=doc('layout',{'formats':arr(enum('PDF','XPS','OXPS'),1),'modelRef':ref('reference'),'identificationRefs':arr(ref('reference'),1),'parserRefs':arr(ref('reference'),1),'validationRefs':arr(ref('reference')),'variants':arr(ref('variant'),1),'metadata':arr(ref('metadata')),'metadataDerivations':arr(ref('metadataDerivation')),'classification':arr(ref('classification')),'classificationPolicy':obj({'evaluation':const('allRules'),'noMatch':const('unrecognizedCandidate'),'incompatibleMatches':const('ambiguous')}),'groups':arr(ref('group')),'completeness':arr(ref('completeness')),'openQuestions':arr(S)})
schemas['parsers']=doc('parsers',{'profiles':arr(ref('parser'),1)})
schemas['model']=doc('model',{'category':enum('AJ','GL','AF','unresolved'),'fields':arr(obj({'id':S,'type':enum('text','identifier','integer','decimal','date','time','accountingPeriod'),'domainTarget':nullable(S)})),'domainMappingStatus':enum('pendingReview','approved')})
schemas['identification']=doc('identification',{'scan':obj({'initialPages':ref('pageScope'),'additionalPages':ref('pageScope'),'expandWhen':const('unresolvedDecision'),'onBudgetExhausted':const('retainCoverageAndUnresolvedDecisions')}),'signals':arr(ref('signal'),1),'evidenceGroups':arr(obj({'id':S,'scoreCap':P,'repeatedOccurrences':const('maximumOnce')}),1),'decisions':arr(ref('decision'),1),'layoutCandidates':arr(ref('reference'),1),'onTie':const('retainCandidates'),'scoreMeaning':const('rankingNotProbability')})
schemas['validation']=doc('validation',{'layoutRef':ref('reference'),'printedControls':arr(ref('control')),'calculations':arr(ref('calculation')),'rules':arr(ref('validationRule'))})
schemas['catalog']=doc('catalog',{'definitions':arr(obj({'ref':ref('reference'),'kind':enum('layout','identification','parsers','validation','model'),'path':S,'sha256':{'type':'string','pattern':'^[a-f0-9]{64}$'}}),1),'geometry':obj({'unit':const('pt'),'origin':const('lowerLeft'),'frame':const('uprightCropLocal'),'rectangleOrder':const(['left','bottom','right','top'])}),'limits':obj({k:{'type':'integer','minimum':1} for k in ['fileBytes','pageCount','decompressedPackageBytes','regexTimeoutMs','predicateDepth','candidateHypotheses','calibrationHypotheses','executionTimeMs']})})
schemas['manifest-row']=obj({'FilePath':S,'AccountingEntity':{'type':'string'},'PerceivedCategory':enum('HL_KNIHA','U_DENNIK','UCT_ROZVRH'),'PreviousTestResult':{'type':'string'},'ExpectedYear':{'type':'string','pattern':'^(|[0-9]{4})$'}})
# Definitions and execution results are separate contracts. Decimal values are strings.
D['diagnostic']=obj({'code':S,'severity':enum('info','warning','error'),'message':S,'evidenceRefs':arr(S),'ruleIds':arr(S)})
D['coverage']=obj({'inspectedPages':arr({'type':'integer','minimum':1}),'availablePageCount':I,'originalInputCompleteness':enum('established','partial','unknown'),'unresolvedCandidateCount':I,'orphanTokenCount':I})
D['value']=obj({'state':enum('present','blank','unresolved','invalid','ambiguous'),'value':nullable(S),'origin':nullable(enum('printed','derived','inherited')),'rawText':{'type':'string'},'evidenceRefs':arr(S),'alternatives':arr(S),'derivationInputs':arr(S)})
D['record']=obj({'id':S,'variantRef':S,'blockRef':S,'semanticClass':S,'exportEligibility':enum('eligible','excluded','review'),'aggregationMemberships':arr(S),'fields':{'type':'object','additionalProperties':ref('value')},'evidenceRefs':arr(S)})
D['evidence']=obj({'id':S,'physicalPageIndex':{'type':'integer','minimum':1},'sourcePageId':S,'sourceElementRefs':arr(S),'rawText':{'type':'string'},'rawGeometry':arr(ref('rect')),'normalizedGeometry':arr(ref('rect')),'transformRefs':arr(S),'ruleIds':arr(S),'blockOccurrenceId':nullable(S),'inheritedFrom':arr(S)})
schemas['result']=obj({'schemaVersion':const(2),'format':enum('PDF','XPS','OXPS','unknown'),'processingStatus':enum('success','incomplete','failed','unsupported','excluded'),'requestedLevels':obj({'extraction':enum('recognition','metadata','records'),'validation':enum('none','completeness','internalConsistency')}),'effectiveLevels':obj({'extraction':enum('recognition','metadata','records'),'validation':enum('none','completeness','internalConsistency')}),'recognitionDecisions':arr(obj({'target':enum('category','producer','layout'),'state':enum('confirmed','tentative','ambiguous','unknown'),'candidates':arr(obj({'id':S,'score':N,'evidenceRefs':arr(S)}))})),'metadata':{'type':'object','additionalProperties':ref('value')},'records':arr(ref('record')),'printedControls':{'type':'object','additionalProperties':ref('value')},'calculations':arr(obj({'ruleId':S,'scopeOccurrenceId':S,'state':enum('pass','fail','inconclusive','notEvaluated'),'value':nullable(S),'inputRefs':arr(S)})),'validationResults':arr(obj({'ruleId':S,'scopeOccurrenceId':S,'state':enum('pass','fail','inconclusive','notEvaluated'),'calculatedValue':nullable(S),'printedValue':nullable(S),'difference':nullable(S),'reason':nullable(S),'evidenceRefs':arr(S)})),'coverage':ref('coverage'),'diagnostics':arr(ref('diagnostic')),'provenance':obj({'documentHash':S,'engineVersion':S,'definitions':arr(obj({'id':S,'version':S,'sha256':S})),'evidence':arr(ref('evidence')),'transforms':arr(obj({'id':S,'sourceToNormalized':tup(N,6),'normalizedToSource':tup(N,6)}))})})
schemas['request']=obj({'schemaVersion':const(2),'extractionLevel':enum('recognition','metadata','records'),'validationLevel':enum('none','completeness','internalConsistency'),'catalogRef':ref('reference'),'inputPath':S,'allowedFormats':arr(enum('PDF','XPS','OXPS'),1)})
schemas['manifest-result']=obj({'manifestRowNumber':{'type':'integer','minimum':1},'expected':schemas['manifest-row'],'actual':obj({'category':nullable(S),'entityCin':nullable(S),'entityName':nullable(S),'fiscalYear':nullable({'type':'integer','minimum':1000,'maximum':9999})}),'processingStatus':enum('success','incomplete','failed','unsupported','excluded'),'recognitionStatus':enum('confirmed','tentative','ambiguous','unknown'),'expectationStatus':enum('pass','fail','inconclusive','notEvaluated'),'validationStatus':enum('pass','fail','inconclusive','notEvaluated'),'recordCount':I,'coverage':ref('coverage'),'standardTotals':arr(obj({'name':S,'scopeOccurrenceId':S,'value':nullable(S),'state':enum('pass','fail','inconclusive','notEvaluated')})),'explanations':arr(S),'detailResultPath':nullable(S)})
for name,schema in dict(common={'$defs':D},**schemas).items():
 out={'$schema':'https://json-schema.org/draft/2020-12/schema','$id':URI+name+'.schema.json',**schema}
 path=ROOT/'schemas'/f'{name}.schema.json';path.parent.mkdir(exist_ok=True);path.write_text(json.dumps(out,ensure_ascii=False,indent=2)+'\n',encoding='utf-8',newline='\n')
print(f'Wrote {len(schemas)+1} schemas')
